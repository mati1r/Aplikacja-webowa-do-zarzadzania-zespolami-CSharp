using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Pages;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _dbContext;
        public UserRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Messages
        public List<User> GetActiveUsersInGroup(int? userId, int? groupId)
        {
            return _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .ToList();
        }

        public List<int> GetIdOfActiveUsersInGroup(int? userId, int? groupId)
        {
            return _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .Select(g => g.user_id).ToList();
        }


        //User Profile
        public UserPersonalDataPartial GetUserPersonalData(int userId)
        {
            return _dbContext.Users.Where(u => u.user_id == userId)
                .Select(u => new UserPersonalDataPartial
                {
                    username = u.username,
                    name = u.name,
                    surname = u.surname
                }).First();
        }

        public JsonResult EditAccountData(UserAccountDataPartial userAccountData, int userId)
        {
            //Get the original and replace what changed
            User originalUserData = _dbContext.Users.Where(u => u.user_id == userId).First();

            //Generate new salt and hash password
            originalUserData.salt = Hash.GenerateSalt(16);
            originalUserData.password = Hash.HashPassword(userAccountData.newPassword, originalUserData.salt);
            _dbContext.Users.Update(originalUserData);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public JsonResult EditPersonalData(UserPersonalDataPartial userPersonalData, int userId)
        {
            //Get the original and replace what changed
            User originalUserData = _dbContext.Users.Where(u => u.user_id == userId).First();
            originalUserData.username = userPersonalData.username;
            originalUserData.name = userPersonalData.name;
            originalUserData.surname = userPersonalData.surname;
            _dbContext.Users.Update(originalUserData);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public User GetUserById(int id)
        {
            return _dbContext.Users.Where(u => u.user_id == id).First();
        }

        public bool IsUsernameTakenUserProfile(string username, int? userId)
        {
            return _dbContext.Users.Where(u => u.user_id != userId).Any(ul => ul.username == username);
        }

        //Password Recovery
        public bool IsAccountWithEmail(string email)
        {
            return _dbContext.Users.Any(u => u.e_mail == email);
        }

        public JsonResult SetNewPasswordEmail(string email, string newPassword)
        {
            User user = new User();
            user = _dbContext.Users.Where(u => u.e_mail == email).First();
            user.salt = Hash.GenerateSalt(16);
            user.password = Hash.HashPassword(newPassword, user.salt);

            _dbContext.Update(user);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        //Register
        public bool IsUsernameTaken(string username)
        {
            return _dbContext.Users.Any(ul => ul.username == username);
        }

        public void CreateAccount(User userData)
        {
            userData.salt = Hash.GenerateSalt(16);
            userData.password = Hash.HashPassword(userData.password, userData.salt);
            _dbContext.Users.Add(userData);
            _dbContext.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }

        public LoginUserDTO GetDataOfLogingUser(List<User> usersList, UserDTO userCredentials)
        {
            return usersList
                .Where(ul => ul.e_mail == userCredentials.e_mail && Hash.VerifyPassword(userCredentials.password, ul.password, ul.salt))
                .Select(u => new LoginUserDTO
                {
                    userId = u.user_id,
                    username = u.username
                })
                .First();
        }
    }
}
