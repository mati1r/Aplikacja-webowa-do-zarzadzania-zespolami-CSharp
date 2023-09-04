﻿using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IUserRepository
    {
        //Messages Page
        List<int> GetIdOfActiveUsersInGroup(int? userId, int? groupId);
        List<User> GetActiveUsersInGroup(int? userId, int? groupId);

        //Password recovery
        JsonResult SetNewPasswordEmail(string email, string newPassword);
        bool IsAccountWithEmail(string email);

        //User Profile
        UserPersonalDataPartial GetUserPersonalData(int userId);
        bool IsUsernameAvailable(string username, int? userId);
        User GetUserById(int id);
        JsonResult EditPersonalData(UserPersonalDataPartial userPersonalData, int userId);
        JsonResult EditAccountData(UserAccountDataPartial userAccountData, int userId);
    }
}
