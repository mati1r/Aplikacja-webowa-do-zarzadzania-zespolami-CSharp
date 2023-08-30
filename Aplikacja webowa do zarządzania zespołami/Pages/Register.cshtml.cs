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
        public User userData { get; set; }
        public string error;

        //OnGet and OnPost methods
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            //Reset the value of the error
            error = "";
            if (!ModelState.IsValid)
            {
                Page();
            }
            else
            {
                var usersList = _dbContext.Users.ToList<User>();
                //Check if there is already a user using this e-mail or this username
                if (usersList.Count(ul => ul.e_mail == userData.e_mail) > 0)
                {
                    error = "Istnieje już użytkownik korzystający z tego adresu e-mail";
                }
                else if (usersList.Count(ul => ul.username == userData.username) > 0)
                {
                    error = "Podana nazwa użytkownika jest już zajęta";
                }
                else
                {
                    //Validate data that user submited, don't chceck name and surname if it was not provided
                    error = UserValidation.IsUserRegisterValid(userData.e_mail, userData.username, userData.password, userData.name, 
                                                               userData.surname, userData.name != null, userData.surname != null);
                    //There is no user that uses those data now check if data are in right format
                    if (error == "")
                    {
                        //Add user to system
                        userData.salt = Hash.GenerateSalt(16);
                        userData.password = Hash.HashPassword(userData.password, userData.salt);
                        _dbContext.Users.Add(userData);
                        _dbContext.SaveChanges();
                        Response.Redirect("/");
                    }
                }              
            }
        }
    }
}
