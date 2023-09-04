using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.LoginModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        public RegisterModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [BindProperty]
        public User userData { get; set; }
        public string error;

        //OnGet and OnPost methods
        public void OnGet()
        {
        }

        public IActionResult OnPost() 
        {
            //Reset the value of the error
            error = "";
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Check if there is already a user using this e-mail or this username
            if (_userRepository.IsAccountWithEmail(userData.e_mail))
            {
                error = "Istnieje już użytkownik korzystający z tego adresu e-mail";
                return Page();
            }

            if (_userRepository.IsUsernameTaken(userData.username))
            {
                error = "Podana nazwa użytkownika jest już zajęta";
                return Page();
            }

            //Validate data that user submited, don't chceck name and surname if it was not provided
            error = UserValidation.IsUserRegisterValid(userData.e_mail, userData.username, userData.password, userData.name, 
                                                        userData.surname, userData.name != null, userData.surname != null);
            //There is no user that uses those data now check if data are in right format
            if (error != "")
            {
                return Page();
            }

            //Add user to system
            _userRepository.CreateAccount(userData);
            return Redirect("/");
            
        }
    }
}
