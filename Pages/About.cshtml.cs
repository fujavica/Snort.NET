using System;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace razor.Pages
{
    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "About page.";
        }
    }
}
