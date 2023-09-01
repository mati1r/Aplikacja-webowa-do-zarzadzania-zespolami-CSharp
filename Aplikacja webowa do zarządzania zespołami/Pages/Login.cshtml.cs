using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.LoginModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LoginModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public LoginModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public class UserDTO
        {
            [Required(ErrorMessage = "Pole email jest wymagane")]
            public string e_mail { get; set; }
            [Required(ErrorMessage = "Pole hasło jest wymagane")]
            public string password { get; set; }
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
                var usersGroupsList = _dbContext.Users_Groups.ToList<User_Group>();
                string userEmail = userCredentials.e_mail;
                string password = userCredentials.password;

                //Check if data submited by user are valid
                error = UserValidation.IsUserLoginValid(userCredentials.e_mail, userCredentials.password);
                if (error == "")
                {
                    //Check if credentials are correct for any avaiable user
                    if (usersList.Count(ul => ul.e_mail == userEmail && Hash.VerifyPassword(password, ul.password, ul.salt)) > 0)
                    {
                        //Get the Id and name of user that is trying to login
                        var userData = usersList
                            .Where(ul => ul.e_mail == userEmail && Hash.VerifyPassword(password, ul.password, ul.salt))
                            .Select(u => new
                            {
                                userId = u.user_id,
                                username = u.username
                            })
                            .First();

                        var ownerList = usersGroupsList.Where(ugl => ugl.role == "owner").Select(ugl => ugl.users_user_id).ToList();

                        //Check if user that is trying to login is a owner of any group if so then log him to one of his groups
                        if (ownerList.Count(ol => ol == userData.userId) > 0)
                        {
                            var ownerGroupId = usersGroupsList.Where(ugl => ugl.users_user_id == userData.userId && ugl.role == "owner")
                                                               .Select(ugl => ugl.users_user_id).First();
                            //Set session propertise
                            SetSessionData("Owner", userData.userId, ownerGroupId, userData.username);
                            Response.Redirect("/Zarzadzanie zadaniami");
                        }
                        //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0
                        else
                        {
                            //Check if user is a part of any group and if he have an active member status
                            if (usersGroupsList.Count(ugl => ugl.users_user_id == userData.userId && ugl.status == "active") > 0)
                            {
                                //Find a group fo a user
                                var userGroupId = usersGroupsList.Where(ugl => ugl.users_user_id == userData.userId && ugl.status == "active" && ugl.role == "user")
                                                                  .Select(ug => ug.groups_group_id).First();
                                //Set session propertise
                                SetSessionData("User", userData.userId, userGroupId, userData.username);
                            }
                            //If user is not part of any group or he is not active in any set session group to 0
                            else
                            {
                                SetSessionData("User", userData.userId, 0, userData.username);
                            }
                            Response.Redirect("/Zadania");
                        }
                    }
                    //If there is no such user in system
                    else
                    {
                        error = "Podane dane są niepoprawne";
                    }
                }
            }
        }
    }
}
