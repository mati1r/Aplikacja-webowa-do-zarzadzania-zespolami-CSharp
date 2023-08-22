using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class User_Group
    {
        public int users_user_id { get; set; }
        public User Users { get; set; }
        public int groups_group_id { get; set; }
        public Group Groups { get; set; }

        public string status { get; set; }

        public string role {get; set;}
    }
}
