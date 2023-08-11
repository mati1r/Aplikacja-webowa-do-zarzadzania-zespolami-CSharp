using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public class UserValidation
    {
        private int minPassword = 4;
        private int maxPassword = 30;
        private int minEmail = 5;
        private int maxEmail = 50;
        private int minUserName = 3;
        private int maxUserName = 20;
        private bool IsEmailLengthValid(string email)
        {
            return (email.Length >= minEmail && email.Length <= maxEmail);
        }
        public bool IsEmailValid(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Console.WriteLine("email");
            Console.WriteLine(IsEmailLengthValid(email) && Regex.IsMatch(email, pattern));

            return IsEmailLengthValid(email) && Regex.IsMatch(email, pattern);
        }

        public bool IsPasswordLenghtValid(string password)
        {
            Console.WriteLine("haslo");
            Console.WriteLine(password.Length >= minPassword && password.Length <= maxPassword);
            return password.Length >= minPassword && password.Length <= maxPassword;
        }
        private bool IsUserNameLengthValid(string userName)
        {
            return (userName.Length >= minUserName && userName.Length <= maxUserName);
        }
        public bool IsUserNameValid(string userName)
        {
            string pattern = "^[a-zA-Z0-9]+$";
            Console.WriteLine("username");
            Console.WriteLine(IsUserNameLengthValid(userName) && Regex.IsMatch(userName, pattern));
            return IsUserNameLengthValid(userName) && Regex.IsMatch(userName, pattern);
        }
    }
}
