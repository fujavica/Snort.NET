using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using snortdb;

namespace razor
{
    public class Country
    {
        public string country { get; set; }
        public int count { get; set; }
        public string code { get; set; }
    }

    public class Content
    {

        public Content(string label, int value, string color = null)
        {
            this.label = label;
            this.value = value;
            this.color = color;
            /*  if(color == null){
                  var r = new Random();
                  int A = r.Next(0, 16777215);
                  string hexValue1 = A.ToString("X");
                  this.color = "#" + hexValue1;              
              }*/
        }
        public string label { get; set; }
        public int value { get; set; }
        public string color { get; set; }
    }

    public class SunContent
    {

        public SunContent(string name)
        {
            this.name = name;
        }
        public SunContent(string name, int size)
        {
            this.name = name;
            this.size = size;
        }
        public string name { get; set; }
        public int number { get; set; }
        // public string value {get;set;}
        public int size { get; set; }
        public List<SunContent> children { get; set; }
    }

    public class MonthData
    {
        public int unit;
        public string date;

        [JsonProperty(PropertyName = "High severity")]
        public int Hig;

        [JsonProperty(PropertyName = "Medium severity")]
        public int Med;

        [JsonProperty(PropertyName = "Low severity")]
        public int Low;
        //public int P4;
    }
    public class SignatureTimeline
    {
        public SignatureTimeline(string name, string color)
        {
            this.name = name;
            this.color = color;
            this.data = new List<Date>();
        }

        public string name;
        public string color;
        public List<Date> data;
    }

    public class Date
    {
        public Date(string date)
        {
            this.date = date;
        }
        public string date;

    }



    public static class Stats
    {
        public static string[] colors_conv =
        {"#e6194B", "#3cb44b", "#ffe119", "#4363d8", "#f58231", "#911eb4", "#42d4f4", "#f032e6", "#bfef45","#fabebe","#469990",
        "#e6beff","#9A6324","#fffac8","#800000","#aaffc3","#808000","#ffd8b1","#000075"/*,"#ffffff"*/,"#000000"};

        public static string[] colors_rain = { "#42d4f4", "#4363d8", "#911eb4",
        "#f032e6", "#e6194B", "#f58231", "#ffe119", "#bfef45", "#3cb44b"};

        public static string SignaturesInTime(List<Alert> alerts, int year = 0, int month = 0, int day = 0)
        {
            List<SignatureTimeline> timeline = new List<SignatureTimeline>();
            Random r = new Random();
            List<string[]> colorsets = new List<string[]>();
            colorsets.Add(colors_conv);
            colorsets.Add(colors_rain);
            string[] colors = colorsets.ElementAt(r.Next(0, colorsets.Count()));
            int colorIndex = 0;

            foreach (var alerts_grouped in alerts.Where(x => (year == 0 || x.time.Year == year) && (month == 0 || x.time.Month == month) && (day == 0 || x.time.Day == day))
            .GroupBy(x => x.sig_id).OrderByDescending(x => x.Count()).ToList())
            {
                SignatureTimeline signatureTL = new SignatureTimeline(alerts_grouped.First().desc, colors[colorIndex]);
                foreach (Alert a in alerts_grouped)
                {
                    signatureTL.data.Add(new Date("new Date('" + a.time.ToString("yyyy/MM/dd HH:mm:ss") + "')"));
                }
                timeline.Add(signatureTL);

                colorIndex++;
                if (colorIndex >= colors.Length) colorIndex = 0;
            }
            return JsonConvert.SerializeObject(timeline).Replace("\"}", "}").Replace("\"new", "new");
        }

        public static string ByYear(List<Alert> alerts, ref List<Alert> filteredAlerts, int year = 0)
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            List<MonthData> months = new List<MonthData>();
            filteredAlerts = alerts.Where(x => x.time.Year == year).ToList();
            months = filteredAlerts.GroupBy(x => x.time.Month)
            .Select(m => new MonthData
            {
                unit = m.First().time.Month,
                date = new DateTime(year, m.First().time.Month, DateTime.DaysInMonth(year, m.First().time.Month)).ToString("o"),
                Hig = m.Where(x => x.priority == 1).Count(),
                Med = m.Where(x => x.priority == 2).Count(),
                Low = m.Where(x => x.priority >= 3).Count(),
                //P4 = m.Where(x => x.priority > 3).Count(),
            }).OrderBy(x => x.unit).ToList();

            return JsonConvert.SerializeObject(months);
        }
        public static string ByMonth(List<Alert> alerts, ref List<Alert> filteredAlerts, int year = 0, int month = 0)
        {

            int? maxDay;
            bool maxDayCorrection = false;
            if (month == 0 && year == 0)
            {
                maxDay = DateTime.Now.Day;
            }
            else
            {
                maxDay = DateTime.DaysInMonth(year, month);
                maxDayCorrection = true;
            }

            if (month == 0)
            {
                month = DateTime.Now.Month;
            }

            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            if (maxDayCorrection)
            {
                maxDay = DateTime.DaysInMonth(year, month);
            }

            List<MonthData> months = new List<MonthData>();
            filteredAlerts = alerts.Where(x => x.time.Year == year && x.time.Month == month).ToList();
            months = filteredAlerts.GroupBy(x => x.time.Day)
            .Select(m => new MonthData
            {
                unit = m.First().time.Day,
                date = new DateTime(year, month, m.First().time.Day).ToString("o"),
                Hig = m.Where(x => x.priority == 1).Count(),
                Med = m.Where(x => x.priority == 2).Count(),
                Low = m.Where(x => x.priority >= 3).Count(),
                //P4 = m.Where(x => x.priority > 3).Count(),
            })
                                    //.OrderBy(x => x.unit)
                                    .ToList();

            for (int i = 1; i <= maxDay; i++)
            {
                if (months.Where(x => x.unit == i).Count() > 0) continue;
                months.Add(new MonthData
                {
                    unit = i,
                    date = new DateTime(year, month, i).ToString("o"),
                });
            }
            months = months.OrderBy(x => x.unit).ToList();
            return JsonConvert.SerializeObject(months);
        }
        public static string ByDay(List<Alert> alerts, ref List<Alert> filteredAlerts, int year = 0, int month = 0, int day = 0)
        {
            int? maxHour;

            if (day == 0 && year == 0 && month == 0)
            {
                maxHour = DateTime.Now.Hour;
            }
            else
            {
                maxHour = 23;
            }

            if (day == 0)
            {
                day = DateTime.Now.Day;
            }
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            if (month == 0)
            {
                month = DateTime.Now.Month;
            }
            List<MonthData> months = new List<MonthData>();
            filteredAlerts = alerts.Where(x => x.time.Year == year && x.time.Month == month && x.time.Day == day).ToList();
            months = filteredAlerts.GroupBy(x => x.time.Hour)
            .Select(m => new MonthData
            {
                unit = m.First().time.Hour,
                date = new DateTime(year, month, day, m.First().time.Hour, 0, 0).ToString("o"),
                Hig = m.Where(x => x.priority == 1).Count(),
                Med = m.Where(x => x.priority == 2).Count(),
                Low = m.Where(x => x.priority >= 3).Count(),
                //P4 = m.Where(x => x.priority > 3).Count(),
            })
                                    //.OrderBy(x => x.unit)
                                    .ToList();

            for (int i = 0; i <= maxHour; i++)
            {
                if (months.Where(x => x.unit == i).Count() > 0) continue;
                months.Add(new MonthData
                {
                    unit = i,
                    date = new DateTime(year, month, day, i, 0, 0).ToString("o"),
                });
            }
            months = months.OrderBy(x => x.unit).ToList();
            return JsonConvert.SerializeObject(months);
        }
        public static string GraphDataByCountry(List<Alert> alerts, Dictionary<string, string> iplocations, out Dictionary<string, int> country_attacks)
        {
            country_attacks = new Dictionary<string, int>();

            string countryISOA3;
            foreach (Alert a in alerts)
            {
                string loc = iplocations[a.src_ip];
                if (!country_attacks.ContainsKey(loc))
                {
                    country_attacks[loc] = 1;
                }
                else
                {
                    country_attacks[loc]++;
                }

            }
            /*
            foreach(string ip in alerts.Select(x => x.src_ip).Distinct()){
                if(iplocations.ContainsKey(ip)){
                    int count = alerts.Where(x => x.src_ip == ip).Count();
                    string country = iplocations[ip];
                        if(!country_attacks.ContainsKey(country)) country_attacks[country] = count;
                        else country_attacks[country]+= count;
                    }
            }*/

            string output;
            output = @"id\tname\tpopulation\t";
            foreach (KeyValuePair<string, int> pair in country_attacks)
            {
                try
                {
                    string countryISOA2 = pair.Key;
                    RegionInfo info = new RegionInfo(countryISOA2);
                    countryISOA3 = info.ThreeLetterISORegionName;
                }
                catch (Exception)
                {
                    countryISOA3 = "n/a";
                }

                output += @"\n";
                output += countryISOA3 + @"\t" + countryISOA3 + @"\t" + pair.Value + @"\t";
            }
            return output;
        }

        public static string ByProtocol(List<Alert> alerts)
        {
            SunContent root = new SunContent("Protocols");
            SunContent tcp = new SunContent("TCP");
            tcp.number = 6;
            SunContent udp = new SunContent("UDP");
            udp.number = 17;
            SunContent icmp = new SunContent("ICMP");
            icmp.number = 1;

            int restCount = 0;
            root.children = new List<SunContent>();
            icmp.children = new List<SunContent>();
            udp.children = new List<SunContent>();
            tcp.children = new List<SunContent>();

            Dictionary<int, int> UDPports = new Dictionary<int, int>();
            Dictionary<int, int> TCPports = new Dictionary<int, int>();
            Dictionary<int, int> ICMPtypes = new Dictionary<int, int>();
            int udpCount = 0;
            int tcpCount = 0;
            int icmpCount = 0;

            int subprotocol;
            foreach (Alert a in alerts)
            {
                switch (a.protocol)
                {
                    case 17:
                        {
                            udpCount++;
                            subprotocol = a.subprotocol_dest;
                            if (!UDPports.ContainsKey(subprotocol))
                            {
                                UDPports.Add(subprotocol, 1);
                            }
                            else
                            {
                                UDPports[subprotocol]++;
                            }
                            break;
                        }
                    case 6:
                        {
                            tcpCount++;
                            subprotocol = a.subprotocol_dest;
                            if (!TCPports.ContainsKey(subprotocol))
                            {
                                TCPports.Add(subprotocol, 1);
                            }
                            else
                            {
                                TCPports[subprotocol]++;
                            }
                            break;
                        }
                    case 1:
                        {
                            icmpCount++;
                            subprotocol = a.subprotocol_dest;
                            if (!ICMPtypes.ContainsKey(subprotocol))
                            {
                                ICMPtypes.Add(subprotocol, 1);
                            }
                            else
                            {
                                ICMPtypes[subprotocol]++;
                            }
                            break;
                        }
                }

            }
            foreach (KeyValuePair<int, int> portCount in UDPports)
            {
                if (portCount.Value < ((double)udpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                udp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            udp.children.Add(new SunContent("Others (<1%)",
                    restCount)
                    );
            root.children.Add(udp);
            restCount = 0;

            foreach (KeyValuePair<int, int> portCount in TCPports)
            {
                if (portCount.Value < ((double)tcpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                tcp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            tcp.children.Add(new SunContent("Others (<1%)",
                     restCount)
                     );
            root.children.Add(tcp);
            restCount = 0;

            foreach (KeyValuePair<int, int> portCount in ICMPtypes)
            {
                if (portCount.Value < ((double)icmpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                icmp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            icmp.children.Add(new SunContent("Others (<1%)",
                    restCount)
                    );
            root.children.Add(icmp);

            return JsonConvert.SerializeObject(root);
            ;
        }
        public static string ByProtocolSource(List<Alert> alerts)
        {
            SunContent root = new SunContent("Protocols");
            SunContent tcp = new SunContent("TCP");
            tcp.number = 6;
            SunContent udp = new SunContent("UDP");
            udp.number = 17;
            SunContent icmp = new SunContent("ICMP");
            icmp.number = 1;

            int restCount = 0;
            root.children = new List<SunContent>();
            icmp.children = new List<SunContent>();
            udp.children = new List<SunContent>();
            tcp.children = new List<SunContent>();

            Dictionary<int, int> UDPports = new Dictionary<int, int>();
            Dictionary<int, int> TCPports = new Dictionary<int, int>();
            Dictionary<int, int> ICMPtypes = new Dictionary<int, int>();
            int udpCount = 0;
            int tcpCount = 0;
            int icmpCount = 0;

            int subprotocol;
            foreach (Alert a in alerts)
            {
                switch (a.protocol)
                {
                    case 17:
                        {
                            udpCount++;
                            subprotocol = a.subprotocol_src;
                            if (!UDPports.ContainsKey(subprotocol))
                            {
                                UDPports.Add(subprotocol, 1);
                            }
                            else
                            {
                                UDPports[subprotocol]++;
                            }
                            break;
                        }
                    case 6:
                        {
                            tcpCount++;
                            subprotocol = a.subprotocol_src;
                            if (!TCPports.ContainsKey(subprotocol))
                            {
                                TCPports.Add(subprotocol, 1);
                            }
                            else
                            {
                                TCPports[subprotocol]++;
                            }
                            break;
                        }
                    case 1:
                        {
                            icmpCount++;
                            subprotocol = a.subprotocol_src;
                            if (!ICMPtypes.ContainsKey(subprotocol))
                            {
                                ICMPtypes.Add(subprotocol, 1);
                            }
                            else
                            {
                                ICMPtypes[subprotocol]++;
                            }
                            break;
                        }
                }

            }
            foreach (KeyValuePair<int, int> portCount in UDPports)
            {
                if (portCount.Value < ((double)udpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                udp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            udp.children.Add(new SunContent("Others (<1%)",
                    restCount)
                    );
            root.children.Add(udp);
            restCount = 0;

            foreach (KeyValuePair<int, int> portCount in TCPports)
            {
                if (portCount.Value < ((double)tcpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                tcp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            tcp.children.Add(new SunContent("Others (<1%)",
                     restCount)
                     );
            root.children.Add(tcp);
            restCount = 0;

            foreach (KeyValuePair<int, int> portCount in ICMPtypes)
            {
                if (portCount.Value < ((double)icmpCount / 100))
                {
                    restCount += portCount.Value;
                    continue;
                }
                icmp.children.Add(new SunContent(
                                                   portCount.Key.ToString(),
                                                   portCount.Value)
                                                   );
            }
            icmp.children.Add(new SunContent("Others (<1%)",
                    restCount)
                    );
            root.children.Add(icmp);

            return JsonConvert.SerializeObject(root);
            ;
        }
        public static string BySignature(List<Alert> alerts, Dictionary<UInt32, string> sigNames)
        {
            List<Content> content = new List<Content>();
            Dictionary<UInt32, int> signatures = new Dictionary<UInt32, int>();
            string snortConf = ProgramData.snort_conf;
            foreach (Alert a in alerts)
            {

                if (!signatures.ContainsKey(a.sig_id))
                {
                    signatures.Add(a.sig_id, 0);
                }
                signatures[a.sig_id]++;
            }


            Random r = new Random();
            List<string[]> colorsets = new List<string[]>();
            colorsets.Add(colors_conv);
            colorsets.Add(colors_rain);
            string[] colors = colorsets.ElementAt(r.Next(0, colorsets.Count()));

            foreach (KeyValuePair<UInt32, int> signature in signatures)
            {
                //int A = r.Next(0, 16777215);
                //string hexValue1 = A.ToString("X");
                content.Add(new Content(sigNames[signature.Key], signature.Value));
            }

            int color = 0;
            content.Sort((A, B) => B.value.CompareTo(A.value));
            foreach (Content c in content)
            {
                string NewColor = colors[color];
                c.color = NewColor;
                color++;
                if (color >= colors.Length) color = 1;
            }
            return JsonConvert.SerializeObject(content);

        }


        public static string ByAttacker(List<Alert> alerts)
        {
            List<Content> content = new List<Content>();
            Dictionary<string, int> attackers = new Dictionary<string, int>();
            string snortConf = ProgramData.snort_conf;
            foreach (Alert a in alerts)
            {

                if (!attackers.ContainsKey(a.src_ip))
                {
                    attackers.Add(a.src_ip, 0);
                }
                attackers[a.src_ip]++;
            }


            Random r = new Random();
            List<string[]> colorsets = new List<string[]>();
            colorsets.Add(colors_conv);
            colorsets.Add(colors_rain);
            string[] colors = colorsets.ElementAt(r.Next(0, colorsets.Count()));

            foreach (KeyValuePair<string, int> attacker in attackers)
            {
                //int A = r.Next(0, 16777215);
                //string hexValue1 = A.ToString("X");
                content.Add(new Content(attacker.Key, attacker.Value));
            }

            int color = 0;
            content.Sort((A, B) => B.value.CompareTo(A.value));
            foreach (Content c in content)
            {
                string NewColor = colors[color];
                c.color = NewColor;
                color++;
                if (color >= colors.Length) color = 1;
            }
            return JsonConvert.SerializeObject(content);

        }
        public static string ByTarget(List<Alert> alerts)
        {
            List<Content> content = new List<Content>();
            Dictionary<string, int> attackers = new Dictionary<string, int>();
            string snortConf = ProgramData.snort_conf;
            foreach (Alert a in alerts)
            {

                if (!attackers.ContainsKey(a.dest_ip))
                {
                    attackers.Add(a.dest_ip, 0);
                }
                attackers[a.dest_ip]++;
            }


            Random r = new Random();
            List<string[]> colorsets = new List<string[]>();
            colorsets.Add(colors_conv);
            colorsets.Add(colors_rain);
            string[] colors = colorsets.ElementAt(r.Next(0, colorsets.Count()));

            foreach (KeyValuePair<string, int> attacker in attackers)
            {
                //int A = r.Next(0, 16777215);
                //string hexValue1 = A.ToString("X");
                content.Add(new Content(attacker.Key, attacker.Value));
            }

            int color = 0;
            content.Sort((A, B) => B.value.CompareTo(A.value));
            foreach (Content c in content)
            {
                string NewColor = colors[color];
                c.color = NewColor;
                color++;
                if (color >= colors.Length) color = 1;
            }
            return JsonConvert.SerializeObject(content);

        }
        public static string GetRandomColor()
        {
            string color = "#";
            color += AddDigitToColor(6);
            for (var i = 0; i < 5; i++)
            {
                color += AddDigitToColor(16);
            }
            return color;
        }

        public static string AddDigitToColor(int limit)
        {
            Random r = new Random();
            int A = r.Next(0, limit);
            string hexValue1 = A.ToString("X");
            return hexValue1;
        }

    }

}