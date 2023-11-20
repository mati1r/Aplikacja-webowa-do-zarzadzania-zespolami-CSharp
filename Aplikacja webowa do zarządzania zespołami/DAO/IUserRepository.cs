using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.DAO
{
    public interface IUserDAO
    {
        //Messages Page
        List<int> GetIdOfActiveUsersInGroup(int? userId, int? groupId);
        List<User> GetActiveUsersInGroupBesidesYourself(int? userId, int? groupId);

        //OwnerTask
        List<User> GetActiveUsersInGroup(int? userId, int? groupId);

        //Password recovery
        void SetNewPasswordEmail(string email, string newPassword);
        bool IsAccountWithEmail(string email);

        //User Profile
        UserPersonalDataPartial GetUserPersonalData(int userId);
        bool IsUsernameTakenUserProfile(string username, int? userId);
        User GetUserById(int id);
        void EditPersonalData(UserPersonalDataPartial userPersonalData, int userId);
        void EditAccountData(UserAccountDataPartial userAccountData, int userId);

        //Register
        bool IsUsernameTaken(string username);
        void CreateAccount(User userData);

        //Login
        List<User> GetAllUsers();
        LoginUserDTO GetDataOfLogingUser(List<User> usersList, UserDTO userCredentials);

        //EditGroup
        List<User> GetPendingUsersList(int groupId);
        List<User> GetActiveUsersList(int groupId, int userId);
    }
}
