using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using snortdb;
using razor.ORM.DAO_MYSQL;

namespace razor.Pages
{
    public static class StaticData
    {
        public static List<Alert> alerts = null;
        public static List<Sensor> sensors = null;

        public static Dictionary<int, string> ref_classes = null;
        public static Dictionary<int, string> class_names = null;
        public static List<Protocol> protocols = null;
        public static List<TransportProtocol> trprotocols = null;
        public static Dictionary<UInt32, string> signatureStrings = null;
        public static Dictionary<string, string> iplocations = new Dictionary<string, string>();
        public static List<Country> country_table = new List<Country>();
    }


    public class IndexModel : PageModel
    {
        int limit = 0;
        public string xLabel;
        public string error = null;
        public string yLabel;
        public string months;
        public DateTime lastTime = DateTime.MinValue;
        public List<Alert> alerts = null;
        public string attackers;
        public string targets;
        public string timeline;
        public string upText; public string upQuery;
        public string leftText; public string leftQuery;
        public string downText; public string downQuery;
        public string middleText;
        public string rightText; public string rightQuery;
        public string timelineStart; public string timelineEnd;
        public string timeFilterStart; public string timeFilterEnd;
        public bool noevents = false;
        public bool noalerts = true;

        public void OnGet()
        {
            try
            {
                SnortContext db = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;

                if (StaticData.sensors == null)
                {
                    StaticData.sensors = SensorTable.GetSensors(db.GetConnection());
                }

                if (StaticData.alerts == null)
                {
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
            }
            catch (Exception e)
            {
                error = "Database error: " + e.Message;
                return;
            }
            try {
            int year = 0, month = 0, day = 0;

            DateTime startdate;
            DateTime enddate;

            Microsoft.Extensions.Primitives.StringValues queryVal;
            if (HttpContext.Request.Query.TryGetValue("view", out queryVal))
            {
                Microsoft.Extensions.Primitives.StringValues yearVal;
                Microsoft.Extensions.Primitives.StringValues monthVal;
                Microsoft.Extensions.Primitives.StringValues dayVal;

                HttpContext.Request.Query.TryGetValue("year", out yearVal);
                HttpContext.Request.Query.TryGetValue("month", out monthVal);
                HttpContext.Request.Query.TryGetValue("day", out dayVal);

                switch (queryVal.FirstOrDefault())
                {
                    case "year":
                        {
                            int.TryParse(yearVal, out year);
                            months = Stats.ByYear(StaticData.alerts, ref alerts, year);
                            if (year == 0)
                            {
                                year = DateTime.Now.Year;
                            }

                            middleText = year.ToString();
                            int upmonth = StaticData.alerts.Where(x => x.time.Year == year).Select(x => x.time.Month).DefaultIfEmpty().Max();
                            upText = upmonth.ToString();
                            upQuery = "?view=month&year=" + year + "&month=" + upmonth;
                            if (year < DateTime.Now.Year)
                            {
                                rightText = (year + 1).ToString();
                                leftQuery = "?view=year&year=" + (year + 1);
                            }

                            if (year > StaticData.alerts.Select(x => x.time.Year).DefaultIfEmpty(year).Min())
                            {
                                leftText = (year - 1).ToString();
                                leftQuery = "?view=year&year=" + (year - 1);
                            }
                            startdate = new DateTime(year, 1, 1);
                            enddate = new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
                            xLabel = year.ToString();
                            yLabel = "%b";
                            break;
                        }
                    case "month":
                        {
                            int.TryParse(yearVal, out year);
                            int.TryParse(monthVal, out month);

                            if (year == 0)
                            {
                                year = DateTime.Now.Year;
                            }
                            if (month == 0)
                            {
                                month = DateTime.Now.Month;
                            }
                            months = Stats.ByMonth(StaticData.alerts, ref alerts, year, month);

                            xLabel = year + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                            yLabel = "%d";

                            middleText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Substring(0, 3) + " " + year;
                            int upday = StaticData.alerts.Where(x => x.time.Year == year && x.time.Month == month).Select(x => x.time.Day).DefaultIfEmpty().Max();
                            upText = upday.ToString();
                            upQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + upday;
                            downText = year.ToString();
                            downQuery = "?view=year&year=" + year;
                            if (month < 12)
                            {
                                if (year == DateTime.Now.Year && month >= DateTime.Now.Month) { }
                                else
                                {
                                    rightText = (month + 1).ToString();
                                    rightQuery = "?view=month&year=" + year + "&month=" + (month + 1);
                                }
                            }
                            if (month > 1)
                            {
                                leftText = (month - 1).ToString();
                                leftQuery = "?view=month&year=" + year + "&month=" + (month - 1);
                            }
                            startdate = new DateTime(year, month, 1);
                            enddate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                            break;
                        }
                    case "day":
                        {
                            int.TryParse(yearVal, out year);
                            int.TryParse(monthVal, out month);
                            int.TryParse(dayVal, out day);

                            if (year == 0)
                            {
                                year = DateTime.Now.Year;
                            }
                            if (month == 0)
                            {
                                month = DateTime.Now.Month;
                            }
                            if (day == 0)
                            {
                                day = DateTime.DaysInMonth(year, month);
                            }
                            months = Stats.ByDay(StaticData.alerts, ref alerts, year, month, day);

                            xLabel = year + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + day;
                            yLabel = "%H";
                            middleText = day + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Substring(0, 3) + " " + year;
                            downText = month.ToString();
                            downQuery = "?view=month&year=" + year + "&month=" + month;
                            if (day > 1)
                            {
                                leftText = (day - 1).ToString();
                                leftQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + (day - 1).ToString();
                            }
                            if (day < DateTime.DaysInMonth(year, month))
                            {
                                if (year == DateTime.Now.Year && month == DateTime.Now.Month && day >= DateTime.Now.Day) { }
                                else
                                {
                                    rightText = (month + 1).ToString();
                                    rightQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + (day + 1).ToString();
                                }
                            }
                            startdate = new DateTime(year, month, day, 0, 0, 0);
                            enddate = new DateTime(year, month, day, 23, 59, 59);
                            break;
                        }
                    default:
                        {
                            year = DateTime.Now.Year;
                            month = DateTime.Now.Month;
                            day = DateTime.Now.Day;

                            months = Stats.ByDay(StaticData.alerts, ref alerts, year, month, day);


                            xLabel = year + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + day;
                            yLabel = "%H";
                            middleText = day + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Substring(0, 3) + " " + year;
                            downText = month.ToString();
                            downQuery = "?view=month&year=" + year + "&month=" + month;
                            if (day > 1)
                            {
                                leftText = (day - 1).ToString();
                                leftQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + (day - 1).ToString();
                            }
                            if (day < DateTime.DaysInMonth(year, month))
                            {
                                if (year == DateTime.Now.Year && month == DateTime.Now.Month && day <= DateTime.Now.Day) { }
                                else
                                {
                                    rightText = (month + 1).ToString();
                                    rightQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + (day + 1).ToString();
                                }
                            }
                            startdate = new DateTime(year, month, day, 0, 0, 0);
                            enddate = new DateTime(year, month, day, 23, 59, 59);
                            break;
                        }
                }
            }
            else
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month;
                //day = DateTime.Now.Day;

                months = Stats.ByMonth(StaticData.alerts, ref alerts, year, month);


                xLabel = year + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                yLabel = "%d";
                middleText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Substring(0, 3) + " " + year;
                int upday = StaticData.alerts.Where(x => x.time.Year == year && x.time.Month == month).Select(x => x.time.Day).DefaultIfEmpty().Max();
                upText = upday.ToString();
                upQuery = "?view=day&year=" + year + "&month=" + month + "&day=" + upday;
                downText = year.ToString();
                downQuery = "?view=year&year=" + year;
                if (month < 12)
                {
                    if (year == DateTime.Now.Year && month >= DateTime.Now.Month) { }
                    else
                    {
                        rightText = (month + 1).ToString();
                        rightQuery = "?view=month&year=" + year + "&month=" + (month + 1);
                    }
                }
                if (month > 1)
                {
                    leftText = (month - 1).ToString();
                    leftQuery = "?view=month&year=" + year + "&month=" + (month - 1);
                }
                startdate = new DateTime(year, month, 1);
                enddate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }

            attackers = Stats.ByAttacker(alerts);
            targets = Stats.ByTarget(alerts);
            timeline = Stats.SignaturesInTime(alerts, year, month, day);
            if (alerts.Count == 0) noevents = true;
            if (StaticData.alerts != null && StaticData.alerts.Count() > 0) noalerts = false;
            alerts = null;
            timelineStart = "new Date('" + startdate.ToString("yyyy/MM/dd HH:mm:ss") + "')";
            timelineEnd = "new Date('" + enddate.ToString("yyyy/MM/dd HH:mm:ss") + "')";
            timeFilterStart = startdate.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            timeFilterEnd = enddate.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

            GC.Collect(1, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                error = "e.Message";
                return;
            }
            }
    }

    public class IndexGridModel : PageModel
    {
        int limit = 10;
        public long lastTime;
        public bool nodata = false;
        public List<Alert> alerts = new List<Alert>();
        public string months;

        public void OnGet(long ls)
        {
            lastTime = ls;
            if (StaticData.alerts != null && StaticData.alerts.Count() != 0)
            {
                alerts = StaticData.alerts.Take(limit).ToList();
            }
            else
            {
                nodata = true;
            }

        }
    }

}