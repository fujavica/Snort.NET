using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace razor
{
    public static class Utils
    {
        public class Tcpdump
        {
            public string path { get; set; }

            public Tcpdump(string path)
            {
                this.path = path;
            }
        }

        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            string result = null;
            try
            {
                process.Start();
                result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception)
            {

            }
            return result;
        }

        public static string ConvertHex(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length - 1; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }

        public static string extractPcap(string file)
        {
            return Bash("tshark -r " + file + " -T fields -E separator=/t -e frame.time_epoch -e _ws.col.No. -e frame.time" +
            " -e _ws.col.Source -e _ws.col.Destination -e _ws.col.Protocol -e _ws.col.Length -e _ws.col.Info");
        }

        public static string GetICMPType(string code)
        {
            string path = Path.Combine(Startup.AppPath ,"wwwroot/ICMP-types.txt");

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.StartsWith("#" + code))
                {
                    return line;
                }
            }
            return "Unknown type";
        }
    }

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }
}