using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using razor.Pages;
using System.Globalization;
using snortdb;
using razor.ORM.DAO_MYSQL;
using MaxMind.GeoIP2;
using System.IO;

namespace razor{

    public static class Geolocator
    {
        public static List<string> ips;
        public static void Geolocate(Dictionary<string, string> IPlocations, SnortContext threadDB)
        {
            var ips = new List<string>(IPlocations.Keys);

            ;
            foreach (string ip in ips)
            {
                if(IPlocations[ip] == "new")
                {
                    using (var reader = new DatabaseReader(Path.Combine("wwwroot/GeoLite2-Country.mmdb")))
                    {

                        string code;
                        string countryISOA3;
                        try { 
                            var country = reader.Country(ip);
                            code = country.Country.IsoCode;
                            string countryISOA2 = code;
                            RegionInfo info = new RegionInfo(countryISOA2);
                            countryISOA3 = info.ThreeLetterISORegionName;
                        }
                        catch (Exception)
                        {
                            countryISOA3 = "n/a";
                            code = "n/a";
                        }

                        StaticData.iplocations[ip] = code;
                        geolocationTable.InsertIP(ip, code, threadDB.GetConnection());
                    }

                //Online geolocation
                    /* using (WebClient wc = new WebClient())
                     {
                         //string url = "http://ip-api.com/json/" + location.Key;
                         //string url = "http://api.hostip.info/get_json.php?ip=" + ip ;

                         string url = "http://www.geoplugin.net/json.gp?ip=" + ip ;
                         string json;
                         try
                         {
                             json = wc.DownloadString(url);
                         }
                         catch (Exception)
                         {
                             return;
                         }
                         try
                         {
                             JObject o = JObject.Parse(json);
                         if (o.ContainsKey("country_code"))
                         {
                             //string country = (string)o["countryCode"];
                             string country = (string)o["country_code"];

                                 //New IP
                                 string countryISOA2 = country;
                                 RegionInfo info = new RegionInfo(countryISOA2);
                                 string countryISOA3 = info.ThreeLetterISORegionName;
                                 StaticData.iplocations[ip] = countryISOA3;
                                 geolocationTable.InsertIP(ip, countryISOA3, threadDB.GetConnection());

                         }
                         }
                         catch (Exception)
                         {
                             StaticData.iplocations[ip] = "n/a";
                             geolocationTable.InsertIP(ip, "n/a", threadDB.GetConnection());
                             return;
                         }*/

  
                }
                

            }
            return;        
            
        }

        
    }
}