using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using snortdb;
using NonFactors.Mvc.Grid;
using System.Linq;
using razor.Pages;

namespace razor{
    public static class Filtering
    {
        public static void applyFilter(ref List<Alert> alerts, IQueryCollection qu){
            NonFactors.Mvc.Grid.Grid<Alert> grid = new NonFactors.Mvc.Grid.Grid<Alert>(alerts); 
            grid.Query = qu;

            System.Linq.Expressions.Expression<Func<Alert, int>> expr = i => i.priority;            
                grid.Columns.Add<int>(expr);
               // Filter = new GridColumnFilter<T, TValue>(this);

            System.Linq.Expressions.Expression<Func<Alert, string>> expr2 = i => i.src_ip;   
                grid.Columns.Add<string>(expr2);

            System.Linq.Expressions.Expression<Func<Alert, string>> expr3 = i => i.dest_ip;   
                grid.Columns.Add<string>(expr3);  

            System.Linq.Expressions.Expression<Func<Alert, string>> expr4 = i => i.desc;   
                grid.Columns.Add<string>(expr4);

            System.Linq.Expressions.Expression<Func<Alert, DateTime>> expr5 = i => i.time;            
                grid.Columns.Add<DateTime>(expr5);

            System.Linq.Expressions.Expression<Func<Alert, int>> expr6 = i => i.protocol;            
                grid.Columns.Add<int>(expr6);

            System.Linq.Expressions.Expression<Func<Alert, UInt16>> expr7 = i => i.subprotocol_dest;            
                grid.Columns.Add<UInt16>(expr7);

            System.Linq.Expressions.Expression<Func<Alert, UInt16>> expr8 = i => i.subprotocol_src;
                grid.Columns.Add<UInt16>(expr8);


            foreach (IGridColumn column in grid.Columns)
            {              
                    column.Filter.IsEnabled = true;
                    column.Filter.IsMulti = true;
            }

            IQueryable<Alert> items = grid.Source;

            foreach(IGridProcessor<Alert> p in grid.Processors){
                items = p.Process(items);
            }         
            alerts = items.ToList();
        }
        public static void applyProtocolFilters(ref List<Alert> alerts, IQueryCollection qu){
            NonFactors.Mvc.Grid.Grid<Alert> grid = new NonFactors.Mvc.Grid.Grid<Alert>(alerts); 
            grid.Query = qu;
                        System.Linq.Expressions.Expression<Func<Alert, int>> expr = i => i.protocol;            
                grid.Columns.Add<int>(expr);
                        System.Linq.Expressions.Expression<Func<Alert, UInt16>> expr2 = i => i.subprotocol_dest;            
                grid.Columns.Add<UInt16>(expr2);

            System.Linq.Expressions.Expression<Func<Alert, UInt16>> expr3 = i => i.subprotocol_src;
            grid.Columns.Add<UInt16>(expr3);
            foreach (IGridColumn column in grid.Columns)
            {              
                    column.Filter.IsEnabled = true;
                    column.Filter.IsMulti = true;
            }
            IQueryable<Alert> items = grid.Source;

            foreach(IGridProcessor<Alert> p in grid.Processors){
                items = p.Process(items);
            }         
            alerts = items.ToList();
        }

    }

}