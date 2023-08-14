﻿using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Users
    {
        [Key]
        public int user_id { get; set; }

        [StringLength(20, MinimumLength = 3, ErrorMessage = "Minimalna długość dla tego pola to 3 a maksymalna to 30 znaków")]
        [Required(ErrorMessage = "Pole login jest wymagane")]
        public string username { get; set; }

        [StringLength(30, MinimumLength = 4, ErrorMessage = "Minimalna długość dla tego pola to 4 a maksymalna to 30 znaków")]
        [Required(ErrorMessage = "Pole hasło jest wymagane")]
        public string password { get; set; }
        public string? salt { get; set; }

        [StringLength(50, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 5 a maksymalna to 50 znaków")]
        [Required(ErrorMessage = "Pole email jest wymagane")]
        public string e_mail { get; set; }

        [StringLength(30, MinimumLength = 5)]
        public string? name { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string? surname { get; set; }

        public ICollection<Users_Groups>? Users_Groups { get; set; }
        public ICollection<Tasks>? Tasks { get; set; }

    }
}
