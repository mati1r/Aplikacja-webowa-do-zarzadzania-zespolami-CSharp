using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.LoginModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public UserProfileModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
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

        private UserPersonalDataPartial GetUserPersonalData(int userId)
        {
           return _dbContext.Users.Where(u => u.user_id == userId)
                .Select(u => new UserPersonalDataPartial
                {
                    username = u.username,
                    name = u.name,
                    surname = u.surname
                }).First();
        }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                userPersonalData = GetUserPersonalData((int)userId);
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
            if (_dbContext.Users.Where(u => u.user_id != userId).Count(ul => ul.username == userPersonalData.username) > 0)
            {
                validationErrors.Add("Podana nazwa użytkownika jest już zajęta");
                return new JsonResult(validationErrors);
            }

            //Get the original and replace what changed
            User originalUserData = _dbContext.Users.Where(u => u.user_id == userId).First();
            originalUserData.username = userPersonalData.username;
            originalUserData.name = userPersonalData.name;
            originalUserData.surname = userPersonalData.surname;
            _dbContext.Users.Update(originalUserData);
            _dbContext.SaveChanges();

            return new JsonResult("success");
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
            User passwordCheck = _dbContext.Users.Where(u => u.user_id == userId).First();
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

            //Get the original and replace what changed
            User originalUserData = _dbContext.Users.Where(u => u.user_id == userId).First();

            //Generate new salt and hash password
            originalUserData.salt = Hash.GenerateSalt(16);
            originalUserData.password = Hash.HashPassword(userAccountData.newPassword, originalUserData.salt);
            _dbContext.Users.Update(originalUserData);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        //Partial methods
        public PartialViewResult OnGetPersonalDataPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                userPersonalData = GetUserPersonalData((int)userId);
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
