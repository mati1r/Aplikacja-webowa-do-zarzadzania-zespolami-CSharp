using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class UserValidation
    {
        //Values to check lenght [same as in model]
        private static int minPassword = 4;
        private static int maxPassword = 30;
        private static int minEmail = 5;
        private static int maxEmail = 50;
        private static int minUserName = 3;
        private static int maxUserName = 20;
        private static int minName = 2;
        private static int maxName = 30;
        private static int minSurname = 5;
        private static int maxSurname = 50;

        private static bool IsLengthValid(string value, int min, int max)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return (value.Length >= min && value.Length <= max);
            }
            else
            {
                return false;
            }
        }
        private static bool IsNameValid(string name) 
        {
            string pattern = "^[A-Z][a-z]*$";
            return IsLengthValid(name, minName, maxName) && Regex.IsMatch(name, pattern);
        }
        private static bool IsSurenameValid(string surname)
        {
            string pattern = "^[A-Z][a-z]*$";
            return IsLengthValid(surname, minSurname, maxSurname) && Regex.IsMatch(surname, pattern);
        }
        private static bool IsEmailValid(string email)
        {
            //Email regex
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return IsLengthValid(email, minEmail, maxEmail) && Regex.IsMatch(email, pattern);
        }

        private static bool IsPasswordValid(string password)
        {
            //Password regex [no space allowd]
            string pattern = @"^[^\s]+$";
            return IsLengthValid(password, minPassword, maxPassword) && Regex.IsMatch(password, pattern);
        }
        private static bool IsUserNameValid(string userName)
        {
            //Username regex [all low and capital letters + numbers]
            string pattern = "^[a-zA-Z0-9]+$";
            return IsLengthValid(userName, minUserName, maxUserName) && Regex.IsMatch(userName, pattern);
        }

        public static string IsUserRegisterValid (string email, string username, string password, string name, string surname, bool isName, bool isSurname)
        {
            if (!IsUserNameValid(username))
                return "Nazwa użytkownika nie spełnia założeń formatowych";

            if (!IsPasswordValid(password))
                return "Hasło nie spełnia założeń formatowych";

            if (!IsEmailValid(email))
                return "Email nie spełnia założeń formatowych";

            if (!IsNameValid(name) && isName)
                return "Imię nie spełnia założeń formatowych";

            if (!IsSurenameValid(surname) && isSurname)
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
    }
}
