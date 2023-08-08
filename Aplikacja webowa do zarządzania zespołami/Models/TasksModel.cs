using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? FinishDate { get; set; }     
        public string Priority { get; set; }
        public string Status { get; set; }
        public string? Feedback { get; set; }
    }
}
