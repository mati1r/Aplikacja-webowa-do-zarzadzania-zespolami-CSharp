using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars
{
    public class UserDTO
    {
        [Required(ErrorMessage = "Pole email jest wymagane")]
        public string e_mail { get; set; }
        [Required(ErrorMessage = "Pole hasło jest wymagane")]
        public string password { get; set; }
    }
}
