using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class MessageValidation
    {
        public static bool IsTopicValid(string topic)
        {
            string pattern = "^[\\p{L}\\d.,!? ]+$";
            return Regex.IsMatch(topic, pattern);
        }


        public static string IsMessageValid(string topic)
        {
            if (!IsTopicValid(topic))
                return "Temat nie spełnia założeń formatowych";

            return "";
        }
    }
}
