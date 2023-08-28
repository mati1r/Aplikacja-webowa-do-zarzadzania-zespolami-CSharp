using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.PartialModels
{
    public class UserPersonalDataPartial
    {
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Minimalna długość dla tego pola to 3 a maksymalna to 30 znaków")]
        [Required(ErrorMessage = "Pole nazwa użytkownika jest wymagane")]
        public string username { get; set; }

        [StringLength(30, MinimumLength = 2, ErrorMessage = "Minimalna długość dla tego pola to 2 a maksymalna to 30 znaków")]
        public string? name { get; set; }
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 5 a maksymalna to 50 znaków")]
        public string? surname { get; set; }
    }
}
