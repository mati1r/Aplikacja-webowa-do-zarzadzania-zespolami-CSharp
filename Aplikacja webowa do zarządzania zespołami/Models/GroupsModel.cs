using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Groups
    {
        [Key]
        public int group_id { get; set; }
        public int owner_id { get; set; }

        [StringLength(50, MinimumLength = 5)]
        [Required(ErrorMessage = "Pole nazwa jest wymagane (minimalna długość to 5 a maksymalna to 30 znaków)")]
        public string name { get; set; }

        [StringLength(200, MinimumLength = 5)]
        [Required(ErrorMessage = "Pole opis jest wymagane (minimalna długość to 5 a maksymalna to 200 znaków)")]
        public string description { get; set; }

        public ICollection<Users_Groups>? Users_Groups { get; set; }
    }
}
