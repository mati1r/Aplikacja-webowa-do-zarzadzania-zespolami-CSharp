using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LoginModel : PageModel
    {

        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;

        public LoginModel(IUserRepository userRepository, IGroupRepository groupRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
        }

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
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                    ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            List<User> usersList = _userRepository.GetAllUsers();
            if (!AreCredentialsCorrect(usersList))
            {
                validationErrors.Add("Podane dane są niepoprawne");
                return new JsonResult(validationErrors);
            }

            //Get the Id and name of user that is trying to login
            LoginUserDTO userData = _userRepository.GetDataOfLogingUser(usersList, userCredentials);

            if (_groupRepository.IsUserAnOwnerOfAnyGroup(userData.userId))
            {
                SetSessionData("Owner", userData.userId, _groupRepository.GetOwnerGroupId(userData.userId), userData.username);
                return new JsonResult("success");
            }
            //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0

            //Check if user is a part of any group and if he have an active member status
            if (_groupRepository.IsUserActiveMemberOfAnyGroup(userData.userId))
            {
                SetSessionData("User", userData.userId, _groupRepository.GetUserGroupId(userData.userId), userData.username);
            }
            //If user is not part of any group or he is not active in any set session group to 0
            else
            {
                SetSessionData("User", userData.userId, 0, userData.username);
            }
            return new JsonResult("success");

        }
    }
}
