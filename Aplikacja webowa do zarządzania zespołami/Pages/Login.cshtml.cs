using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

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
                var groupsList = _dbContext.Groups.ToList<Groups>();
                string userEmail = userCredentials.e_mail;
                string password = userCredentials.password;

                //Get id's of all admins
                var ownerList = groupsList.Select(c=>c.owner_id).ToList();
                //Check if credentials are correct for any avaiable user
                if (usersList.Count(c => c.e_mail == userEmail && c.password == password) > 0)
                {
                    //Get the Id of user that is trying to login
                    Users user = usersList.Where(c => c.e_mail == userEmail && c.password == password).First();
                    int userId = user.user_id;
                    //Check if user that is trying to login is a owner of a group if so then log him to his group
                    int ownerId = ownerList.Count(c => c == userId);
                    if (ownerId > 0) 
                    {
                        var GroupId = groupsList.Where(c => c.owner_id == ownerId).Select(c => c.group_id).First();
                        Console.WriteLine(GroupId.ToString());
                        //HttpContext.Session.SetString(Key, "Owner");
                        //HttpContext.Session.SetInt32(Key2, UserId);
                        Response.Redirect("/AdminTasks");
                    }
                    //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0
                    else
                    {
                        //Check if user is a part of any group
                        if (_dbContext.Users_Groups.Count(c => c.users_user_id == userId) > 0)
                        {
                            //Find a group fo a user
                            var UserGroup = _dbContext.Users_Groups.Where(c => c.users_user_id == userId).Select(c => c.groups_group_id).First();
                            Console.WriteLine("Grupa użytkownika = "+ UserGroup.ToString());
                            //Set session propertise
                            //HttpContext.Session.SetString(Key, "User");
                            //HttpContext.Session.SetInt32(Key2, UserId);
                        }
                        //If user is not part of any group set session group to 0
                        else
                        {
                            Console.WriteLine("Grupa użytkownika = 0");
                        }
                        Response.Redirect("/UserTasks");
                    }
                }
                //If there is no such user in system
                else
                {
                    Page();
                    error = "Podane dane są niepoprawne";
                }
            }
        }
    }
}
