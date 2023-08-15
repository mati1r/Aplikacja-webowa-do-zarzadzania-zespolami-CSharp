using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class UserValidation
    {
        private static int minPassword = 4;
        private static int maxPassword = 30;
        private static int minEmail = 5;
        private static int maxEmail = 50;
        private static int minUserName = 3;
        private static int maxUserName = 20;
        private static bool IsLengthValid(string value, int min, int max)
        {
            return (value.Length >= min && value.Length <= max);
        }
        public static bool IsEmailValid(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return IsLengthValid(email,minEmail,maxEmail) && Regex.IsMatch(email, pattern);
        }

        public static bool IsPasswordLenghtValid(string password)
        {
            return password.Length >= minPassword && password.Length <= maxPassword;
        }
        public static bool IsUserNameValid(string userName)
        {
            string pattern = "^[a-zA-Z0-9]+$";
            return IsLengthValid(userName, minUserName, maxUserName) && Regex.IsMatch(userName, pattern);
        }
    }
}
