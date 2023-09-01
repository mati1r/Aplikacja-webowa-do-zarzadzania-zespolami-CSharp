namespace Aplikacja_webowa_do_zarządzania_zespołami.PartialModels
{
    public class GroupQuitPartial
    {
        public int group_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string owner_name { get; set; }
        public string role { get; set; }

        public string status { get; set; }
    }
}
