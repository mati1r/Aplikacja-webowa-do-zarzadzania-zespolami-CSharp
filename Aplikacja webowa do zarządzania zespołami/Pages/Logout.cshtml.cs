using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LogoutModel : PageModel
    {
        public string data;
        public void OnGet()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/");
        }
    }
}
