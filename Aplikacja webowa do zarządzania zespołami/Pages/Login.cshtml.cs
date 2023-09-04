using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LoginModel : PageModel
    {

        private readonly IUserRepository _userRepository;

        public LoginModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string error;


        [BindProperty]
        public UserDTO userCredentials { get; set; }

        private void SetSessionData(string userType, int userId, int groupId , string userName)
        {
            HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), userType);
            HttpContext.Session.SetInt32(ConstVariables.GetKeyValue(2), userId);
            HttpContext.Session.SetInt32(ConstVariables.GetKeyValue(3), groupId);
            HttpContext.Session.SetString(ConstVariables.GetKeyValue(4), userName);
        }

        //OnGet and OnPost methods
        public void OnGet()
        {
        }

        //Private methods
        private bool AreCredentialsCorrect(List<User> usersList)
        {
            return usersList.Any(ul => ul.e_mail == userCredentials.e_mail && Hash.VerifyPassword(userCredentials.password, ul.password, ul.salt));
        }

        public IActionResult OnPost() 
        {
            //Reset the value of the error
            error = "";
            if (!ModelState.IsValid)
            {
                return Page();
            }

            List<User> usersList = _userRepository.GetAllUsers();
            if (!AreCredentialsCorrect(usersList))
            {
                error = "Podane dane są niepoprawne";
                return Page();
            }

            //Get the Id and name of user that is trying to login
            LoginUserDTO userData = _userRepository.GetDataOfLogingUser(usersList, userCredentials);

            if (_userRepository.IsUserAnOwner(userData))
            {
                SetSessionData("Owner", userData.userId, _userRepository.GetOwnerGroupId(userData), userData.username);
                return Redirect("/Zarzadzanie zadaniami");
            }
            //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0

            //Check if user is a part of any group and if he have an active member status
            if (_userRepository.IsUserActiveMemberOfGroup(userData))
            {
                SetSessionData("User", userData.userId, _userRepository.GetUserGroupId(userData), userData.username);
            }
            //If user is not part of any group or he is not active in any set session group to 0
            else
            {
                SetSessionData("User", userData.userId, 0, userData.username);
            }
            return Redirect("/Zadania");           
            
        }
    }
}
