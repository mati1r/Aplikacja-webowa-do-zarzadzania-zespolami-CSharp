using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class UserValidation
    {
        private static bool IsNameValid(string name, bool isName) 
        {
            if (isName)
            {
                string pattern = "^[A-Z][a-z]*$";
                return Regex.IsMatch(name, pattern);
            }
            else
            {
                return true;
            }
        }
        private static bool IsSurenameValid(string surname, bool isSurname)
        {
            if (isSurname)
            {
                string pattern = "^[A-Z][a-z]*$";
                return Regex.IsMatch(surname, pattern);
            }
            else
            {
                return true;
            }
        }
        private static bool IsEmailValid(string email)
        {
            //Email regex
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        private static bool IsPasswordValid(string password)
        {
            //Password regex (no space allowed)
            string pattern = @"^[^\s]+$";
            return Regex.IsMatch(password, pattern);
        }
        private static bool IsUserNameValid(string userName)
        {
            //Username regex (all low and capital letters + numbers)
            string pattern = "^[a-zA-Z0-9]+$";
            return Regex.IsMatch(userName, pattern);
        }

        public static string IsUserRegisterValid (string email, string username, string password, string name, string surname, bool isName, bool isSurname)
        {
            if (!IsUserNameValid(username))
                return "Nazwa użytkownika nie spełnia założeń formatowych";

            if (!IsPasswordValid(password))
                return "Hasło nie spełnia założeń formatowych";

            if (!IsEmailValid(email))
                return "Email nie spełnia założeń formatowych";

            if (!IsNameValid(name, isName))
                return "Imię nie spełnia założeń formatowych";

            if (!IsSurenameValid(surname, isSurname))
                return "Nazwisko nie spełnia założeń formatowych";

            return "";
        }

        public static string IsUserLoginValid(string email, string password)
        {
            if (!IsPasswordValid(password))
                return "Hasło nie spełnia założeń formatowych";

            if (!IsEmailValid(email))
                return "Email nie spełnia założeń formatowych";
                   
            return "";
            
        }

        public static string IsUserPersonalDataValid(string username, string name, string surname, bool isName, bool isSurname)
        {
            if (!IsUserNameValid(username))
                return "Nazwa użytkownika nie spełnia założeń formatowych";

            if (!IsNameValid(name, isName))
                return "Imię nie spełnia założeń formatowych";

            if (!IsSurenameValid(surname, isSurname))
                return "Nazwisko nie spełnia założeń formatowych";

            return "";
        }

        public static string IsNewPasswordValid(string newPassword, string newPasswordRepeat)
        {
            if(newPassword != newPasswordRepeat)
            {
                return "Błędnie powtórzone hasło";
            }
            if (!IsPasswordValid(newPassword))
            {
                return "Hasło nie spełnia założeń formatowych";
            }

            return "";
        }
    }
}
