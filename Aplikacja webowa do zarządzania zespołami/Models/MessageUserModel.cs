namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Message_User
    {
        public int users_user_id { get; set; }
        public User Users { get; set; }
        public int messages_message_id { get; set; }
        public Message Messages { get; set; }
    }
}
