using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Groups
    {
        [Key]
        public int group_id { get; set; }

        [StringLength(50, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 2 a maksymalna to 50 znaków")]
        [Required(ErrorMessage = "Pole nazwa jest wymagane")]
        public string name { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 5 a maksymalna to 200 znaków")]
        [Required(ErrorMessage = "Pole opis jest wymagane")]
        public string description { get; set; }

        public ICollection<Users_Groups>? Users_Groups { get; set; }
        public ICollection<Tasks>? Tasks { get; set; }
    }
}
