using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Users
    {
        [Key]
        public int user_id { get; set; }

        [StringLength(20, MinimumLength = 3)]
        [Required(ErrorMessage = "Pole login jest wymagane (minimalna długość to 3 a maksymalna to 30 znaków)")]
        public string username { get; set; }

        [StringLength(30, MinimumLength = 4)]
        [Required(ErrorMessage = "Pole hasło jest wymagane (minimalna długość to 4 a maksymalna to 30 znaków)")]
        public string password { get; set; }

        [StringLength(50, MinimumLength = 5)]
        [Required(ErrorMessage = "Pole email jest wymagane (minimalna długość to 5 a maksymalna to 50 znaków)")]
        public string e_mail { get; set; }

        [StringLength(30, MinimumLength = 5)]
        public string? name { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string? surname { get; set; }

        public ICollection<Users_Groups>? Users_Groups { get; set; }

    }
}
