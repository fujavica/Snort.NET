using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace razor.Pages
{
    public class IndexContentModel : PageModel
    {
        public bool loading = false;
        public void OnGet()
        {
            if (StaticData.alerts == null)
            {
                loading = true;
            }
        }
    }
}