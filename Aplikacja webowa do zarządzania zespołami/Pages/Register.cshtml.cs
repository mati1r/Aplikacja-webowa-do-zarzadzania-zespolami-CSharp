using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.LoginModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class RegisterModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public RegisterModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public Users userData { get; set; }
        public string error;
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            if (!ModelState.IsValid)
            {
                Page();
            }
            else
            {
                var usersList = _dbContext.Users.ToList<Users>();
                //Check if there is already a user using this e-mail or this username
                if (usersList.Count(c => c.e_mail == userData.e_mail) > 0)
                {
                    error = "Istnieje już użytkownik korzystający z tego adresu e-mail";
                }
                else if (usersList.Count(c => c.username == userData.username) > 0)
                {
                    error = "Podana nazwa użytkownika jest już zajęta";
                }
                else
                {
                    //There is no user that uses those data now check if data are in right format
                    UserValidation validator = new UserValidation();
                    if(validator.IsEmailValid(userData.e_mail) && validator.IsPasswordLenghtValid(userData.password) && validator.IsUserNameValid(userData.username))
                    {
                        //Add user to system
                        _dbContext.Users.Add(userData);
                        _dbContext.SaveChanges();
                        Response.Redirect("/");
                    }
                    else
                    {
                        error = "Podane dane nie są w niepoprawnym formacie";
                    }

                }              
            }
        }
    }
}
