using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.PartialModels
{
    public class UserAccountDataPartial
    {
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Minimalna długość dla tego pola to 4 a maksymalna to 30 znaków")]
        [Required(ErrorMessage = "Pole stare hasło jest wymagane")]
        public string oldPassword { get; set; }

        [StringLength(30, MinimumLength = 4, ErrorMessage = "Minimalna długość dla tego pola to 4 a maksymalna to 30 znaków")]
        [Required(ErrorMessage = "Pole nowe hasło jest wymagane")]
        public string newPassword { get; set; }

        [Required(ErrorMessage = "Pole powtórz hasło jest wymagane")]
        public string newPasswordRepeat { get; set; }
        public string? salt { get; set; }
    }
}
