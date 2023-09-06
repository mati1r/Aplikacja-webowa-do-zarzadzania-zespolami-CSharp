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

        //OnGet and OnPost methods
        public void OnGet()
        {
        }

        public IActionResult OnPost() 
        {
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                    ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            //Check if there is already a user using this e-mail or this username
            if (_userRepository.IsAccountWithEmail(userData.e_mail))
            {
                validationErrors.Add("Istnieje już użytkownik korzystający z tego adresu e-mail");
                return new JsonResult(validationErrors);
            }

            if (_userRepository.IsUsernameTaken(userData.username))
            {
                validationErrors.Add("Podana nazwa użytkownika jest już zajęta");
                return new JsonResult(validationErrors);
            }

            //Validate data that user submited, don't chceck name and surname if it was not provided
            string formatError = UserValidation.IsUserRegisterValid(userData.e_mail, userData.username, userData.password, userData.name, 
                                                        userData.surname, userData.name != null, userData.surname != null);
            //There is no user that uses those data now check if data are in right format
            if (formatError != "")
            {
                validationErrors.Add(formatError);
                return new JsonResult(validationErrors);
            }

            //Add user to system
            _userRepository.CreateAccount(userData);
            return new JsonResult("success");
            
        }
    }
}
