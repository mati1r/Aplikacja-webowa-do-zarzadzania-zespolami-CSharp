using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Message
    {
        [Key]
        public int message_id { get; set; }

        public int groups_group_id { get; set; }
        public Group? Groups { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Minimalna długość dla tego pola to 3 a maksymalna to 50 znaków")]
        [Required(ErrorMessage = "Pole temat jest wymagane")]
        public string topic { get; set; }

        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 5 a maksymalna to 1000 znaków")]
        [Required(ErrorMessage = "Pole zawartość wiadomości jest wymagane")]
        public string content { get; set; }

        public int sender_id { get; set; }

        public DateTime send_date { get; set; }

        public ICollection<Message_User>? Messages_Users { get; set; }

    }
}
