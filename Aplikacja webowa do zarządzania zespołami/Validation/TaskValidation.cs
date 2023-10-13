using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class TaskValidation
    {
        private static int minDescription = 5;
        private static int maxDescription = 1000;
        private static int minTaskName = 3;
        private static int maxTaskName = 100;


        private static bool IsLengthValid(string value, int min, int max)
        {
            return (value.Length >= min && value.Length <= max);
        }
        public static bool IsStatusValid(string status)
        {
            if (status == "nieukończone" || status == "w trakcie" || status == "ukończone") {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPriorityValid(string priority)
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

        public static bool IsTaskNameValid(string taskName)
        {
            string pattern = "^[\\p{L}\\d.,!? ]+$";
            return IsLengthValid(taskName, minTaskName, maxTaskName) && Regex.IsMatch(taskName, pattern);
        }
        public static bool IsDescriptionValid(string description)
        {
            string pattern = "^[\\p{L}\\d.,!? ]+$";
            return IsLengthValid(description, minDescription, maxDescription) && Regex.IsMatch(description, pattern);
        }

        public static string IsTaskValid(string taskName, string status, string priority, string description)
        {
            if (!IsTaskNameValid(taskName))
                return "Nazwa zadania jest nie poprawna";

            if (!IsDescriptionValid(description))
                return "Opis zadania jest nie poprawny";

            if (!IsPriorityValid(priority))
                return "Stopień ważności jest nie poprawny";

            if (!IsStatusValid(status))
                return "Status zadania jest nie poprawny";

            return "";
        }
    }
}
