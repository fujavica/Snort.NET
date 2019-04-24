using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using snortdb;
using NonFactors.Mvc.Grid;
using razor.ORM.DAO_MYSQL;

namespace razor.Pages
{
    public class EventsModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    public class IndexGridModelBig : PageModel
    {

        int limit = 0;
        public List<Alert> alerts;
        public DateTime lastTime = DateTime.MinValue;

        public void OnGet()
        {
            SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;
            if (StaticData.sensors == null)
            {
                StaticData.sensors = SensorTable.GetSensors(db.GetConnection());
            }

            StaticData.sensors = SensorTable.GetSensors(db.GetConnection());
            if (StaticData.alerts == null)
            {
                //List<Event> events = db.GetEvents(limit);
                //StaticData.alerts = AlertBuilder.resolveEvents(events, db);

                StaticData.alerts = AlertMapper.ResolveAlerts(limit, ref StaticData.signatureStrings, db.GetConnection());
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
                        //List<Event> events = EventTable.UpdateEvents(s.sid, lastEvent, db.GetConnection());
                        new_alerts.AddRange(AlertMapper.UpdateAlerts(s.sid, lastEvent, StaticData.signatureStrings, db.GetConnection()));
                    }
                }
                foreach (Alert a in new_alerts)
                {
                    if (StaticData.iplocations.ContainsKey(a.src_ip))
                    {
                        continue;
                    }
                    else StaticData.iplocations[a.src_ip] = "new";
                }
                new_alerts.Sort((alertA, alertB) => DateTime.Compare(alertB.time, alertA.time));
                StaticData.alerts.InsertRange(0, new_alerts);
            }

            //APPLY PROTOCOL FILTERS:/* */
            alerts = StaticData.alerts;
            Filtering.applyProtocolFilters(ref alerts, HttpContext.Request.Query);

        }

    }
    public class ShortNameProcessor : IGridProcessor<Alert>
    {
        public GridProcessorType ProcessorType { get; set; }

        public ShortNameProcessor()
        {
            ProcessorType = GridProcessorType.Pre;
            // Executed before data is filtered and sorted, for low memory footprint of signature strings
        }

        public IQueryable<Alert> Process(IQueryable<Alert> items)
        {
            foreach (var c in items)
            {
                c.desc = StaticData.signatureStrings[c.sig_id];
            }
            return items;
        }
    }
}