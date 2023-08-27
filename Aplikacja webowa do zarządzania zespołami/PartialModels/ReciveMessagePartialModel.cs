namespace Aplikacja_webowa_do_zarządzania_zespołami.PartialModels
{
    public class ReciveMessagePartial
    {
        public int message_id { get; set; }
        public string topic { get; set; }
        public string content { get; set; }
        public DateTime send_date { get; set; }
        public string sender_name { get; set; }
    }
}
