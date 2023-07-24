using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            Response.Redirect("/Index");
        }
    }
}
