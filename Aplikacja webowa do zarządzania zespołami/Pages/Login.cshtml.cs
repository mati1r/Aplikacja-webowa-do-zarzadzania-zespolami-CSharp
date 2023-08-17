using Aplikacja_webowa_do_zarządzania_zespołami.Models;
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
        public class UserDTO
        {
            [Required(ErrorMessage = "Pole email jest wymagane")]
            public string e_mail { get; set; }
            [Required(ErrorMessage = "Pole hasło jest wymagane")]
            public string password { get; set; }
        }

        private readonly DatabaseContext _dbContext;

        public LoginModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public UserDTO userCredentials { get; set; }
        public string error;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";

        public void OnGet()
        {
        }

        private void SetSessionData(string userType, int userId, int groupId)
        {
            HttpContext.Session.SetString(Key, userType);
            HttpContext.Session.SetInt32(Key2, userId);
            HttpContext.Session.SetInt32(Key3, groupId);
            Console.WriteLine("Typu uzytkownika = " + userType);
            Console.WriteLine("Id uzytkownika = " + userId);
            Console.WriteLine("Id grupy = " + groupId);
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
                var usersList = _dbContext.Users.ToList<Users>();
                var groupsList = _dbContext.Groups.ToList<Groups>();
                var usersGroupsList = _dbContext.Users_Groups.ToList<Users_Groups>();
                string userEmail = userCredentials.e_mail;
                string password = userCredentials.password;

                //Get id's of all admins
                var ownerList = groupsList
                    .Select(c=>c.owner_id)
                    .ToList();
                //Check if data submited by user are valid
                error = UserValidation.IsUserLoginValid(userCredentials.e_mail, userCredentials.password);
                if (error == "")
                {
                    //Check if credentials are correct for any avaiable user
                    if (usersList.Count(c => c.e_mail == userEmail && Hash.VerifyPassword(password, c.password, c.salt)) > 0)
                    {
                        //Get the Id of user that is trying to login
                        int userId = usersList
                            .Where(c => c.e_mail == userEmail && Hash.VerifyPassword(password, c.password, c.salt))
                            .Select(u => u.user_id)
                            .First();

                        //Check if user that is trying to login is a owner of a group if so then log him to his group
                        if (ownerList.Count(c => c == userId) > 0)
                        {
                            var ownerGroupId = groupsList
                                .Where(c => c.owner_id == userId)
                                .Select(c => c.group_id)
                                .First();
                            //Set session propertise
                            SetSessionData("Owner", userId, ownerGroupId);
                            Response.Redirect("/AdminTasks");
                        }
                        //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0
                        else
                        {
                            //Check if user is a part of any group and if he have an active member status
                            if (usersGroupsList.Count(c => c.users_user_id == userId && c.status == "aktywny") > 0)
                            {
                                //Find a group fo a user
                                var userGroupId = usersGroupsList.Where(c => c.users_user_id == userId && c.status == "aktywny").Select(c => c.groups_group_id).First();
                                //Set session propertise
                                SetSessionData("User", userId, userGroupId);
                            }
                            //If user is not part of any group or he is not active in any set session group to 0
                            else
                            {
                                SetSessionData("User", userId, 0);
                            }
                            Response.Redirect("/UserTasks");
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
