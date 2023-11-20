using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.LoginModel;
using Aplikacja_webowa_do_zarządzania_zespołami.DAO;
using System.Runtime.InteropServices;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly IUserDAO _userDAO;
        public UserProfileModel(IUserDAO userDAO)
        {
            _userDAO = userDAO;
            userPersonalData = new UserPersonalDataPartial();
            userAccountData = new UserAccountDataPartial();
        }

        public string data;
        public int? userId;
        public int? groupId;
        public string? username;

        [BindProperty]
        public UserPersonalDataPartial userPersonalData { get; set; }

        [BindProperty]
        public UserAccountDataPartial userAccountData { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                userPersonalData = _userDAO.GetUserPersonalData((int)userId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostPersonalDataEdit()
        {
            string error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            List<string> validationErrors = new List<string>();

            //Need to clear ModelState to validate only the userPersonalData model
            ModelState.Clear();
            if (!TryValidateModel(userPersonalData))
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            error = UserValidation.IsUserPersonalDataValid(userPersonalData.username, userPersonalData.name,
                                                           userPersonalData.surname, userPersonalData.name != null, userPersonalData.surname != null);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            //Check if there is any other user with that username (exluding us)
            if (_userDAO.IsUsernameTakenUserProfile(userPersonalData.username, userId))
            {
                validationErrors.Add("Podana nazwa użytkownika jest już zajęta");
                return new JsonResult(validationErrors);
            }

            //Check if user didn't deleted session
            try
            {
                _userDAO.EditPersonalData(userPersonalData, (int)userId);
                return new JsonResult("success");
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostAccountDataEdit()
        {
            string error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            List<string> validationErrors = new List<string>();

            //Need to clear ModelState to validate only the userAccountData model
            ModelState.Clear();
            if (!TryValidateModel(userAccountData))
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            //Check if old password is correct (first need to get current one)
            User passwordCheck = new User();
            try
            {
                passwordCheck = _userDAO.GetUserById((int)userId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }

            if (!Hash.VerifyPassword(userAccountData.oldPassword, passwordCheck.password, passwordCheck.salt))
            {
                validationErrors.Add("Podane stare hasło jest nie poprawne");
                return new JsonResult(validationErrors);
            }

            error = UserValidation.IsNewPasswordValid(userAccountData.newPassword, userAccountData.newPasswordRepeat);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            try
            {
                _userDAO.EditAccountData(userAccountData, (int)userId);
                return new JsonResult("success");
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        //Partial methods
        public PartialViewResult OnGetPersonalDataPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                userPersonalData = _userDAO.GetUserPersonalData((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialUserPersonalData", userPersonalData);
        }


        public PartialViewResult OnGetAccountDataPartial()
        {
            return Partial("Partials/_PartialUserAccountData", userAccountData);
        }
    }
}
