using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using snortdb;
using NonFactors.Mvc.Grid;
using razor.ORM.DAO_MYSQL;

namespace razor.Pages
{
    public class ProtocolsModel : PageModel
    {

        public string protocols;
        public string protocols_source;
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
            }

            alerts = StaticData.alerts;
            if (HttpContext.Request.Query.Count > 0)
            {
                Filtering.applyFilter(ref alerts, HttpContext.Request.Query);
                filtered = true;
            }

            protocols = Stats.ByProtocol(alerts);
            protocols_source = Stats.ByProtocolSource(alerts);
            ;



        }
    }
}
