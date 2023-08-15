using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class TaskValidation
    {
        private static int minDescription = 5;
        private static int maxDescription = 1000;
        private static int minTaskName = 3;
        private static int maxTaskName = 100;

        public static bool isStatusValid(string status)
        {
            if (status == "nieukończone" || status == "ukończone") {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isPriorityValid(string priority)
        {
            if (priority == "niski" || priority == "normalny" || priority == "wysoki")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private static bool IsLengthValid(string value, int min, int max)
        {
            return (value.Length >= min && value.Length <= max);
        }
        public static bool IsTaskNameValid(string taskName)
        {
            string pattern = "^[a-zA-Z0-9 ]+$";
            return IsLengthValid(taskName, minTaskName, maxTaskName) && Regex.IsMatch(taskName, pattern);
        }
        public static bool IsDescriptionValid(string description)
        {
            string pattern = "^[a-zA-Z0-9 ]+$";
            return IsLengthValid(description, minDescription, maxDescription) && Regex.IsMatch(description, pattern);
        }
    }
}
