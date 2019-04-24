using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using snortdb;
using razor.ORM.DAO_MYSQL;

namespace razor.Pages
{
    public class OriginModel : PageModel
    {
        public string Message { get; set; }
        public bool countries = false;
        public string JSONcountries = null;
        public DateTime lastTime = DateTime.MinValue;

        public List<Alert> alerts = new List<Alert>();
        public bool filtered = false;

        public void OnGet()
        {
            SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;

            if (StaticData.sensors == null)
            {
                StaticData.sensors = SensorTable.GetSensors(db.GetConnection());
            }

            if (StaticData.alerts == null)
            {
                StaticData.alerts = AlertMapper.ResolveAlerts(0, ref StaticData.signatureStrings, db.GetConnection());
                foreach (Alert a in StaticData.alerts)
                {
                    if (StaticData.iplocations.ContainsKey(a.src_ip))
                    {
                        continue;
                    }
                    else StaticData.iplocations[a.src_ip] = "new";
                }
            }

            else
            {
                lastTime = StaticData.alerts.Select(x => x.time).DefaultIfEmpty(DateTime.MinValue).Max();
                //Check for new events
                List<Alert> new_alerts = new List<Alert>();
                Dictionary<int, int> lastAlers = new Dictionary<int, int>();
                foreach (Sensor s in StaticData.sensors)
                {
                    int lastEvent = StaticData.alerts.Where(x => x.sid == s.sid).Select(x => x.cid).DefaultIfEmpty(0).Max();
                    if (s.last_cid != lastEvent)
                    {
                        new_alerts.AddRange(AlertMapper.UpdateAlerts(s.sid, lastEvent, StaticData.signatureStrings, db.GetConnection()));
                    }
                }
                new_alerts.Sort((alertA, alertB) => DateTime.Compare(alertB.time, alertA.time));
                foreach (Alert a in new_alerts)
                {
                    if (StaticData.iplocations.ContainsKey(a.src_ip))
                    {
                        continue;
                    }
                    else StaticData.iplocations[a.src_ip] = "new";
                }
                StaticData.alerts.InsertRange(0, new_alerts);
            }
            if (HttpContext.Request.Query.Count > 0)
            {
                //Filtering.applyFilter(ref alerts, HttpContext.Request.Query);
                filtered = true;
            }
        }

    }
}
