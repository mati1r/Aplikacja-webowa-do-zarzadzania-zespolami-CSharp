namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages.DTO_models_and_static_vars
{
    public static class ConstVariables
    {
        private const string Key = "_userType";
        private const string Key2 = "_userId";
        private const string Key3 = "_groupId";
        private const string Key4 = "_userName";

        public static string GetKeyValue(int KeyNumber)
        {
            switch (KeyNumber)
            {
                case 1:
                    return Key;
                case 2:
                    return Key2;
                case 3:
                    return Key3;
                case 4:
                    return Key4;
                default:
                    return "";
            }
        }
    }
}
