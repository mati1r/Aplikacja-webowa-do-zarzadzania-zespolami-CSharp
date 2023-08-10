using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class LoginModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public class UserDTO
        {
            [StringLength(50, MinimumLength = 5)]
            [Required(ErrorMessage = "Pole email jest wymagane (minimalna długość to 5 a maksymalna to 50 znaków)")]
            public string e_mail { get; set; }
            [StringLength(30, MinimumLength = 4)]
            [Required(ErrorMessage = "Pole hasło jest wymagane (minimalna długość to 5 a maksymalna to 30 znaków)")]
            public string password { get; set; }
        }

        public LoginModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public UserDTO user_credentials { get; set; }
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
                var UserList = _dbContext.Users.ToList<Users>();
                var GroupList = _dbContext.Groups.ToList<Groups>();
                int Userlog = 0;
                string UserEmail = user_credentials.e_mail;
                string Password = user_credentials.password;

                //Check if credentials are correct for any avaiable user
                Userlog = UserList.Count(c => c.e_mail == UserEmail && c.password == Password);
                //Get id's of all admins
                var OwnerList = GroupList.Select(c=>c.owner_id).ToList();
                //
                if (Userlog > 0)
                {
                    Users s = new Users();
                    //Get the Id of user that is trying to login
                    s = UserList.Where(c => c.e_mail == UserEmail && c.password == Password).First();
                    int UserId = s.user_id;
                    //Check if user that is trying to login is a owner of a group if so then log him to his group
                    int OwnerId = OwnerList.Count(c => c == UserId);
                    if (OwnerId > 0) 
                    {
                        var GroupId = GroupList.Where(c => c.owner_id == OwnerId).Select(c => c.group_id).First();
                        Console.WriteLine(GroupId.ToString());
                        //HttpContext.Session.SetString(Key, "Owner");
                        //HttpContext.Session.SetInt32(Key2, UserId);
                        Response.Redirect("/AdminTasks");
                    }
                    //If user is not an admin try to log him to group that he is a part of, if there is no such a group set session group as 0
                    else
                    {
                        //Check if user is a part of any group
                        if (_dbContext.Users_Groups.Count(c => c.users_user_id == UserId) > 0)
                        {
                            //Find a group fo a user
                            var UserGroup = _dbContext.Users_Groups.Where(c => c.users_user_id == UserId).Select(c => c.groups_group_id).First();
                            Console.WriteLine("Grupa użytkownika = "+ UserGroup.ToString());
                            //Set session propertise
                            //HttpContext.Session.SetString(Key, "User");
                            //HttpContext.Session.SetInt32(Key2, UserId);
                        }
                        //If not set session group to 0
                        else
                        {
                            Console.WriteLine("Grupa użytkownika = 0");
                        }
                        Response.Redirect("/UserTasks");
                    }
                }
                else
                {
                    Page();
                    error = "Podane dane są nie poprawne";
                }
            }
        }
    }
}
