using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Tasks
    {
        public int users_user_id { get; set; }
        public int groups_group_id { get; set; }
        [Key]
        public int task_id { get; set; }
        public string task_name { get; set; }
        public string description { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public DateTime? finish_date { get; set; }     
        public string priority { get; set; }
        public string status { get; set; }
        public string? feedback { get; set; }
    }
}
