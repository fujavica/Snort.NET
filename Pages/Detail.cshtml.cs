using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using snortdb;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Xml;
using SharpPcap;
using SharpPcap.LibPcap;
using razor;
using System.Threading;
using System.Text;
using razor.ORM.DAO_MYSQL;

namespace razor.Pages
{
    public class DetailModel : PageModel
    {
        public int sid;
        public int cid;
        public Event eve;
        public List<Alert> alerts = new List<Alert>();
        public List<Iphdr> iphdrs = new List<Iphdr>();
        public List<Signature> signatures = new List<Signature>();
        public List<Tcphdr> tcphdrs = new List<Tcphdr>();
        public List<Udphdr> udphdrs = new List<Udphdr>();
        public List<Icmphdr> icmphdrs = new List<Icmphdr>();
        public List<Data> datas = new List<Data>();
        public string data;
        public string className = null;
        public List<string> errors = null;
        public List<AttributeOutput> whoisData = new List<AttributeOutput>();
        public List<AttributeOutput> whoisData2 = new List<AttributeOutput>();
        public string whoisURL;
        public string whoisURL2;
        public List<snortdb.Ref> sigrefs;
        public long targetSec;
        public long targetMS;
        public CaptureFileWriterDevice captureFileWriter;
        private SemaphoreSlim signal;

        public void device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            captureFileWriter.Close();
            signal.Release();
        }
        public void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var content = e.Packet.Data;
            /* if(e.Packet.Timeval.Seconds == (ulong)this.targetSec)
             {
                 if(e.Packet.Timeval.MicroSeconds == ((ulong)this.targetMS % 1000000))
                 {
                     Console.WriteLine("Found PACKET MATCH (uSEC)");
                     Console.WriteLine("TARGET TS > " + this.targetMS + " - FOUND > " + e.Packet.ToString());
                 }
                 Console.WriteLine("Found PACKET MATCH (SEC)");
                 Console.WriteLine("TARGET TS > " + targetSec + " FOUND > " + e.Packet.ToString());
             }*/

            captureFileWriter.Write(e.Packet);
        }

        public async Task<ActionResult> OnPostPcap(int cid, int sid, string source, string dest, string output)
        {
            SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;
            Utils.Tcpdump tcpdump_path = HttpContext.RequestServices.GetService(typeof(Utils.Tcpdump)) as Utils.Tcpdump;
            string path = tcpdump_path.path;
            if (StaticData.alerts == null)
            {
                StaticData.alerts = AlertMapper.ResolveAlerts(0, ref StaticData.signatureStrings, db.GetConnection());
            }

            //alerts = SessionExtensions.Get<List<Alert>>(HttpContext.Session,"alert");
            this.cid = cid;
            this.sid = sid;
            alerts.Add(StaticData.alerts.Where(x => x.cid == cid && x.sid == sid).FirstOrDefault());

            targetSec = ((DateTimeOffset)alerts.First().time).ToUnixTimeSeconds();

            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            targetMS = alerts.First().time.Ticks - epochTicks;
            //TimeSpan epochTicks2 = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            //targetMS = (ulong) (((DateTimeOffset)alerts.First().time).Ticks - epochTicks2.Ticks)/10;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(path, "tcpdump.log.*");
            }
            catch (Exception)
            {
                return RedirectToPage("Error", "Issue", new { issue = "Snort packet log folder not found (" + path + "). Change the path in appsettings.json to match the Snort output path." });
            }

            long closestTS = 0;
            string closestFile = "";

            foreach (string f in files)
            {
                //ulong currentTS = Convert.ToUInt64(f.Split('.').Last());
                long currentTSS = Convert.ToInt64(f.Split('.').Last());
                //DateTime currentTS = Convert.ToDateTime(f.Split('.').Last());
                if (currentTSS <= targetSec && currentTSS > closestTS)
                {
                    closestTS = currentTSS;
                    closestFile = f;
                }
            }
            if (closestFile == "")
            {
                return RedirectToPage("Error", "Issue", new { issue = "No appropriate packet log found in " + path + ". Please review your Snort output configuration and activate: output log_tcpdump: tcpdump.log" });
            }

            if (!System.IO.File.Exists("wwwroot/pcaps/" + sid + "." + cid))
            {

                (new FileInfo("wwwroot/pcaps/" + sid + "." + cid)).Directory.Create();
                //CaptureDeviceList devices = CaptureDeviceList.Instance;
                CaptureFileReaderDevice device = new CaptureFileReaderDevice(closestFile);
                captureFileWriter = new CaptureFileWriterDevice("wwwroot/pcaps/" + sid + "." + cid);
                captureFileWriter.Open();
                device.OnPacketArrival += new PacketArrivalEventHandler(this.device_OnPacketArrival);
                device.OnCaptureStopped += new CaptureStoppedEventHandler(this.device_OnCaptureStopped);
                device.Filter = "host " + source + " and host " + dest;
                device.StartCapture();

                signal = new SemaphoreSlim(0, 1);
                await signal.WaitAsync();
            }
            switch (output)
            {
                case "pcap":
                    {
                        return File("/pcaps/" + sid + "." + cid, "application/octet-stream",
                        sid + "." + cid + ".pcap");
                    }
                case "tcpdump":
                    {
                        string dir = Directory.GetCurrentDirectory();
                        string tcpdump = Utils.Bash("tcpdump -r " + dir + "/wwwroot/pcaps/" + sid + "." + cid);
                        return File(new MemoryStream(Encoding.UTF8.GetBytes(tcpdump ?? "")), "application/octet-stream",
                        sid + "." + cid + ".txt");
                    }
                default:
                    {
                        return File("/pcaps/" + sid + "." + cid, "application/octet-stream",
                        sid + "." + cid + ".pcap");
                    }

            }
        }


        public void OnGet()
        {

            try
            {
                cid = Convert.ToInt32(HttpContext.Request.Query["cid"].ToString());
                sid = Convert.ToInt32(HttpContext.Request.Query["sid"].ToString());
            }
            catch (Exception)
            {
                errors = new List<string>();
                errors.Add("How did you get here? Invalid url.");
                //errors.Add(e.Message);
                return;
            }

            SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;

            if (StaticData.alerts == null)
            {
                StaticData.alerts = AlertMapper.ResolveAlerts(0, ref StaticData.signatureStrings, db.GetConnection());
            }
            //Check for new alerts
            else
            {
            }

            if (StaticData.ref_classes == null)
            {
                StaticData.ref_classes = Reference_systemTable.GetRefClasses(db.GetConnection());
            }
            if (StaticData.class_names == null)
            {
                StaticData.class_names = Sig_classTable.GetClassNames(db.GetConnection());
            }
            if (StaticData.protocols == null)
            {
                StaticData.protocols = XmlUtils.GetProcotols();
                //StaticData.protocols = db.GetProtocols();
            }
            if (StaticData.trprotocols == null)
            {
                StaticData.trprotocols = XmlUtils.GetTransportProcotols();
            }

            alerts.Add(StaticData.alerts.Where(x => x.cid == cid && x.sid == sid).FirstOrDefault());
            //SessionExtensions.Set<List<Alert>>(HttpContext.Session,"alert", alerts);

            eve = EventTable.GetEvent(cid, sid, db.GetConnection());
            if (eve.cid == 0)
            {
                errors = new List<string>();
                errors.Add("How did you get here? Event not found.");
                //errors.Add(e.Message);
                return;
            }

            //SIGNATURE           
            Signature signature = SignatureTable.GetSignature(eve.signature, db.GetConnection());
            if (signature.sig_class_id > 0)
            {
                StaticData.class_names.TryGetValue(signature.sig_class_id, out className);
                signature.class_name = className;
                signatures.Add(signature);
            }
            sigrefs = ReferenceTable.GetReference(signature.sig_id, db.GetConnection());
            if (sigrefs != null)
                foreach (snortdb.Ref sigref in sigrefs)
                {
                    string ref_url = StaticData.ref_classes.GetValueOrDefault(sigref.ref_system_id) + sigref.ref_tag;
                    if (!ref_url.StartsWith("http"))
                        signature.ref_url += "<a href=http://" + ref_url + " target=\"_blank\">" + ref_url + "</a></br>";
                    else
                        signature.ref_url += "<a href=" + ref_url + " target=\"_blank\">" + ref_url + "</a></br>";
                }
            else
            {
                signature.ref_url = "-";
            }

            //IP HEADER
            Iphdr iphdr = IphdrTable.GetIphdr(cid, sid, db.GetConnection());
            if (iphdr.source == null) iphdr.source = AlertMapper.ResolveIP(iphdr.ip_src);
            if (iphdr.destination == null) iphdr.destination = AlertMapper.ResolveIP(iphdr.ip_dst);
            string protocol = StaticData.protocols.Where(x => x.pid == iphdr.ip_proto).Select(x => x.name).FirstOrDefault();
            string proto_ref = StaticData.protocols.Where(x => x.pid == iphdr.ip_proto).Select(x => x.reference).FirstOrDefault();
            if (protocol != null)
            {
                if (proto_ref == null)
                    iphdr.protocol = protocol;
                else
                    iphdr.protocol = "<a href=\"" + proto_ref + "\" target=\"_blank\">" + protocol + "</a>";
            }
            else
            {
                iphdr.protocol = iphdr.ip_proto.ToString();
            }
            iphdrs.Add(iphdr);


            switch (iphdr.ip_proto)
            {
                case 1:      //ICMP HEADER
                    {
                        Icmphdr icmphdr = IcmphdrTable.GetIcmphdr(cid, sid, db.GetConnection());
                        string path = System.AppDomain.CurrentDomain.BaseDirectory + "ICMP-types.txt";

                        //icmphdr.icmp_type_text = Utils.Bash("cat " + path + " | grep '#" + icmphdr.icmp_type + " –'");
                        icmphdr.icmp_type_text = Utils.GetICMPType(icmphdr.icmp_type.ToString());
                        icmphdrs.Add(icmphdr);
                        break;
                    }

                case 6:      //TCP HEADER
                    {
                        Tcphdr tcphdr = TcphdrTable.GetTcphdr(cid, sid, db.GetConnection());
                        //var output = Utils.Bash("cat /etc/services | grep [[:space:]]" + tcphdr.tcp_sport + "/tcp");
                        //var output2 = Utils.Bash("cat /etc/services | grep [[:space:]]" + tcphdr.tcp_dport + "/tcp");
                        TransportProtocol trp = StaticData.trprotocols.Where(x => x.number == tcphdr.tcp_sport && x.protocol == "tcp").FirstOrDefault();
                        TransportProtocol trp2 = StaticData.trprotocols.Where(x => x.number == tcphdr.tcp_dport && x.protocol == "tcp").FirstOrDefault();
                        /*if(output != "")    */
                        if (trp != null)
                        {
                            if (trp.xref != null)
                                tcphdr.tcp_protocol = "<a href=\"" + trp.xref + "\" target=\"_blank\">" + trp.name + "</a>";
                            else
                                tcphdr.tcp_protocol = trp.name;
                            // tcphdr.tcp_protocol = tcphdr.tcp_sport + " ("+ output.Split('\t')[0] + ")";
                            // if(output.Split('#').Count() > 1) tcphdr.tcp_protocol += " – " + output.Split('#')[1];
                        }
                        else tcphdr.tcp_protocol = tcphdr.tcp_sport.ToString();
                        /*if(output2 != "") */
                        if (trp2 != null)
                        {
                            if (trp2.xref != null)
                                tcphdr.tcp_protocol2 = "<a href=\"" + trp2.xref + "\" target=\"_blank\">" + trp2.name + "</a>";
                            else
                                tcphdr.tcp_protocol2 = trp2.name;
                            // tcphdr.tcp_protocol2 = tcphdr.tcp_dport + " ("+ output2.Split('\t')[0] + ")";
                            // if(output2.Split('#').Count() > 1) tcphdr.tcp_protocol2 += " – " + output2.Split('#')[1];
                        }
                        else tcphdr.tcp_protocol2 = tcphdr.tcp_dport.ToString();
                        tcphdrs.Add(tcphdr);
                        break;
                    }

                case 17:      //UDP HEADER
                    {
                        Udphdr udphdr = UdphdrTable.GetUdphdr(cid, sid, db.GetConnection());
                        //var output = Utils.Bash("cat /etc/services | grep [[:space:]]" + udphdr.udp_sport + "/udp");
                        //var output2 = Utils.Bash("cat /etc/services | grep [[:space:]]" + udphdr.udp_dport + "/udp");

                        TransportProtocol trp = StaticData.trprotocols.Where(x => x.number == udphdr.udp_sport && x.protocol == "udp").FirstOrDefault();
                        TransportProtocol trp2 = StaticData.trprotocols.Where(x => x.number == udphdr.udp_dport && x.protocol == "udp").FirstOrDefault();
                        /*if(output != "") */
                        if (trp != null)
                        {

                            //  udphdr.udp_protocol = udphdr.udp_sport + " ("+ output.Split('\t')[0] + ")";
                            //  if(output.Split('#').Count() > 1) udphdr.udp_protocol +=  " – " + output.Split('#')[1];
                            if (trp.xref != null)
                                udphdr.udp_protocol = "<a href=\"" + trp.xref + "\" target=\"_blank\">" + trp.name + "</a>";
                            else
                                udphdr.udp_protocol = trp.name;
                        }
                        else udphdr.udp_protocol = udphdr.udp_sport.ToString();


                        /*if(output2 != "") */
                        if (trp2 != null)
                        {
                            //udphdr.udp_protocol2 = udphdr.udp_dport + " ("+ output2.Split('\t')[0] + ")";
                            //if(output2.Split('#').Count() > 1) udphdr.udp_protocol2 +=  " – " + output2.Split('#')[1];
                            if (trp2.xref != null)
                                udphdr.udp_protocol2 = "<a href=\"" + trp2.xref + "\" target=\"_blank\">" + trp2.name + "</a>";
                            else
                                udphdr.udp_protocol2 = trp2.name;
                        }
                        else udphdr.udp_protocol2 = udphdr.udp_dport.ToString();
                        udphdrs.Add(udphdr);
                        break;
                    }
                default:
                    {
                        break;
                    }

            }


            //WHOIS - RIPE
            using (WebClient wc = new WebClient())
            {
                snortdb.Attributes attList = new snortdb.Attributes();
                try     //SOURCE
                {
                    string url = "https://rest.db.ripe.net/search.xml?query-string=" + iphdr.source +
                            "&flags=no-filtering&source=RIPE";
                    var json = wc.DownloadString(url);

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(json);
                    XmlNode root = xdoc.DocumentElement;
                    attList.attributes = new List<snortdb.Attribute>();
                    foreach (XmlNode record in root.SelectNodes("objects/object"))
                    {
                        if (record.Attributes["type"].Value == "inetnum" || record.Attributes["type"].Value == "inet6num" || record.Attributes["type"].Value == "person"
                         || record.Attributes["type"].Value == "route")
                        {
                            foreach (XmlNode att in record.SelectNodes(@"attributes/attribute"))
                            {
                                if (att.Attributes["name"].Value == "remarks") continue;
                                attList.attributes.Add(new snortdb.Attribute(att.Attributes["name"].Value, att.Attributes["value"].Value));
                            }
                        }
                    }
                    if (attList.attributes.Count() > 0)
                    {
                        whoisURL = "https://apps.db.ripe.net/db-web-ui/#/query?searchtext=" + iphdr.source + "&source=RIPE&bflag=true";
                        attList.attributes.Add(new snortdb.Attribute("source url", "<a href=\"" + whoisURL + "\" target=\"_blank\">" + whoisURL + "<a>"));
                        whoisData.Add(new AttributeOutput(String.Join("<br>", attList.attributes.Select(x => x.name)), String.Join("<br>", attList.attributes.Select(x => x.value))));
                    }

                }
                catch (Exception) { }

                attList.attributes.Clear();
                try     //DESTINATION
                {

                    string url = "https://rest.db.ripe.net/search.xml?query-string=" + iphdr.destination +
                            "&flags=no-filtering&source=RIPE";
                    var json = wc.DownloadString(url);

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(json);
                    XmlNode root = xdoc.DocumentElement;
                    attList.attributes = new List<snortdb.Attribute>();
                    foreach (XmlNode record in root.SelectNodes("objects/object"))
                    {
                        if (record.Attributes["type"].Value == "inetnum" || record.Attributes["type"].Value == "inet6num" || record.Attributes["type"].Value == "person"
                         || record.Attributes["type"].Value == "route")
                        {
                            foreach (XmlNode att in record.SelectNodes(@"attributes/attribute"))
                            {
                                if (att.Attributes["name"].Value == "remarks") continue;
                                attList.attributes.Add(new snortdb.Attribute(att.Attributes["name"].Value, att.Attributes["value"].Value));
                            }
                        }
                    }
                    if (attList.attributes.Count() > 0)
                    {
                        whoisURL2 = "https://apps.db.ripe.net/db-web-ui/#/query?searchtext=" + iphdr.destination + "&source=RIPE&bflag=true";
                        attList.attributes.Add(new snortdb.Attribute("source url", "<a href=\"" + whoisURL2 + "\" target=\"_blank\">" + whoisURL2 + "<a>"));
                        whoisData2.Add(new AttributeOutput(String.Join("<br>", attList.attributes.Select(x => x.name)), String.Join("<br>", attList.attributes.Select(x => x.value))));
                    }

                }
                catch (Exception) { }
            }

            //SessionExtensions.Set<List<Data>>(HttpContext.Session,"datas", datas);



        }
    }
    public class DetailModelData : PageModel
    {
        public List<Data> datas = new List<Data>();
        public string data;
        int sidn;
        int cidn;
        public void OnGet(string cid, string sid)
        {
            try
            {
                try
                {
                    cidn = Convert.ToInt32(cid);
                    sidn = Convert.ToInt32(sid);
                }
                catch (Exception)
                {
                    return;
                }
                //DATA
                SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;

                data = DataTable.GetData(cidn, sidn, db.GetConnection());
                if (data == null) return;
                int totalLength = data.Length;
                int start = 0;
                int length = 0;
                int bytesPerLine = 32;
                while (start < totalLength - 1)
                {
                    if (start + (bytesPerLine * 2) >= totalLength) length = totalLength - start - 1;
                    else length = (bytesPerLine * 2);
                    datas.Add(new Data(data.Substring(start, length)));
                    start += length;
                }

                foreach (Data d in datas)
                {
                    d.ascii = Utils.ConvertHex(d.hex);
                    d.hex = Regex.Replace(d.hex, ".{2}", "$0 ");
                }
                //datas = SessionExtensions.Get<List<Data>>(HttpContext.Session,"datas");
            }
            catch (Exception) {; }

        }
    }

    public class DetailModelWS : PageModel
    {
        public int sid;
        public int cid;
        public List<WSFields> wsFields = new List<WSFields>();
        public List<Alert> alerts = new List<Alert>();
        public long targetSec;
        public long targetMS;
        public CaptureFileWriterDevice captureFileWriter;
        private SemaphoreSlim signal;
        public void OnGet(string cid, string sid)
        {
            try
            {
                try
                {
                    this.cid = Convert.ToInt32(cid);
                    this.sid = Convert.ToInt32(sid);
                }
                catch (Exception)
                {
                    return;
                }

                extractFromPcap(this.cid, this.sid);
                string dir = Directory.GetCurrentDirectory();
                string tsharkInput = Utils.extractPcap(dir + "/wwwroot/pcaps/" + sid + "." + cid);

                using (StringReader reader = new StringReader(tsharkInput))
                {
                    string line = string.Empty;
                    do
                    {
                        line = reader.ReadLine();
                        if (line != null)
                        {
                            string[] fields = line.Split("\t");
                            WSFields ws = new WSFields();

                            try
                            {
                                ws.timestamp = Convert.ToInt64(fields[0].Split(".")[0]);
                            }
                            catch (Exception) { }
                            try
                            {
                                ws.number = Convert.ToInt32(fields[1]);
                                ws.time = fields[2];
                                ws.source = fields[3];
                                ws.destination = fields[4];
                                ws.protocol = fields[5];
                                ws.length = fields[6];
                                ws.info = fields[7];
                            }
                            catch (Exception) { }

                            wsFields.Add(ws);
                            // do something with the line
                        }

                    } while (line != null);
                }
            }
            catch (Exception) {; }
        }
        public async void extractFromPcap(int cid, int sid)
        {
            SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;
            Utils.Tcpdump tcpdump_path = HttpContext.RequestServices.GetService(typeof(Utils.Tcpdump)) as Utils.Tcpdump;
            string path = tcpdump_path.path;
            if (StaticData.alerts == null)
            {
                StaticData.alerts = AlertMapper.ResolveAlerts(0, ref StaticData.signatureStrings, db.GetConnection());
            }

            //alerts = SessionExtensions.Get<List<Alert>>(HttpContext.Session,"alert");
            this.cid = cid;
            this.sid = sid;
            alerts.Add(StaticData.alerts.Where(x => x.cid == cid && x.sid == sid).FirstOrDefault());

            //UTC
            targetSec = ((DateTimeOffset)alerts.First().time).ToUnixTimeSeconds();

            //Local Time
            //TimeSpan epochSecs = new TimeSpan(new DateTime(1970, 1, 1).Second);
            //targetSec = (((DateTimeOffset)alerts.First().time).Second - epochSecs.Seconds);

            if (!System.IO.File.Exists("wwwroot/pcaps/" + sid + "." + cid))
            {
                string source = StaticData.alerts.Where(x => x.cid == cid && x.sid == sid).FirstOrDefault().src_ip;
                string dest = StaticData.alerts.Where(x => x.cid == cid && x.sid == sid).FirstOrDefault().dest_ip;

                long epochTicks = new DateTime(1970, 1, 1).Ticks;
                targetMS = alerts.First().time.Ticks - epochTicks;
                //TimeSpan epochTicks2 = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
                //targetMS = (ulong) (((DateTimeOffset)alerts.First().time).Ticks - epochTicks2.Ticks)/10;

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(path, "tcpdump.log.*");


                    long closestTS = 0;
                    string closestFile = "";

                    foreach (string f in files)
                    {
                        //ulong currentTS = Convert.ToUInt64(f.Split('.').Last());
                        long currentTSS = Convert.ToInt64(f.Split('.').Last());
                        //DateTime currentTS = Convert.ToDateTime(f.Split('.').Last());
                        if (currentTSS <= targetSec && currentTSS > closestTS)
                        {
                            closestTS = currentTSS;
                            closestFile = f;
                        }
                    }

                (new FileInfo("wwwroot/pcaps/" + sid + "." + cid)).Directory.Create();
                    //CaptureDeviceList devices = CaptureDeviceList.Instance;
                    CaptureFileReaderDevice device = new CaptureFileReaderDevice(closestFile);
                    captureFileWriter = new CaptureFileWriterDevice("wwwroot/pcaps/" + sid + "." + cid);
                    captureFileWriter.Open();
                    device.OnPacketArrival += new PacketArrivalEventHandler(this.device_OnPacketArrival);
                    device.OnCaptureStopped += new CaptureStoppedEventHandler(this.device_OnCaptureStopped);
                    device.Filter = "host " + source + " and host " + dest;
                    device.StartCapture();

                    signal = new SemaphoreSlim(0, 1);
                    await signal.WaitAsync();
                }
                catch (Exception) {; }
            }
        }
        public void device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            captureFileWriter.Close();
            signal.Release();
        }
        public void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var content = e.Packet.Data;
            captureFileWriter.Write(e.Packet);
        }
    }


}