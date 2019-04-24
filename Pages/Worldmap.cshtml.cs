using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using razor.ORM.DAO_MYSQL;
using snortdb;

namespace razor.Pages
{
    public class WorldmapModel : PageModel
    {
        public string JSONcountries = null;
        public Dictionary<string, int> country_attacks;
        public List<Country> country_table;
        public List<Alert> alerts = new List<Alert>();

        public void OnGet()
        {
            SnortContext DB = HttpContext.RequestServices.GetService(typeof(SnortContext)) as SnortContext;
            geolocationTable.LoadIPLocations(ref StaticData.iplocations, DB.GetConnection());

            Geolocator.Geolocate(StaticData.iplocations, DB);

            alerts = StaticData.alerts;
            if (HttpContext.Request.Query.Count > 0)
            {
                Filtering.applyFilter(ref alerts, HttpContext.Request.Query);
            }
            JSONcountries = Stats.GraphDataByCountry(alerts, StaticData.iplocations, out country_attacks);

            RegionInfo info;
            country_table = new List<Country>();

            foreach (KeyValuePair<string, int> pair in country_attacks.OrderByDescending(x => x.Value))
            {
                if (pair.Key == "n/a")
                {
                    country_table.Add(new Country
                    {
                        country = pair.Key,
                        count = pair.Value
                    }
                    );
                    continue;
                }
                info = new RegionInfo(pair.Key);
                country_table.Add(new Country
                {
                    country = info.EnglishName,
                    count = pair.Value
                }
                );
            }
            StaticData.country_table = country_table;
            ;
        }
    }

    public class CountryGridModel : PageModel
    {
        public List<Country> country_table;

        public void OnGet()
        {
            country_table = StaticData.country_table;
        }
    }
}