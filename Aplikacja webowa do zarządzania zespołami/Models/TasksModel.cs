using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Tasks
    {
        [Required(ErrorMessage = "Należy wybrać użytkonika")]
        public int users_user_id { get; set; }
        public Users? Users { get; set; }
        public int groups_group_id { get; set; }
        public Groups? Groups { get; set; }
        [Key]
        public int task_id { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Minimalna długość dla tego pola to 3 a maksymalna to 100 znaków")]
        [Required(ErrorMessage = "Pole nazwa zadania jest wymagane")]
        public string task_name { get; set; }
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Minimalna długość dla tego pola to 5 a maksymalna to 1000 znaków")]
        [Required(ErrorMessage = "Pole opis zadania jest wymagane")]
        public string description { get; set; }
        [Required(ErrorMessage = "Pole data zadania jest wymagane")]
        public DateTime start_date { get; set; }
        [Required(ErrorMessage = "Pole data planowanego wykonania jest wymagane")]
        public DateTime end_date { get; set; }
        public DateTime? finish_date { get; set; }
        [Required(ErrorMessage = "Pole stopień ważności jest wymagane")]
        public string priority { get; set; }
        public string status { get; set; }
        public string? feedback { get; set; }
    }
}
