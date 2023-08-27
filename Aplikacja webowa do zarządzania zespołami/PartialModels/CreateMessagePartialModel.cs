using Aplikacja_webowa_do_zarządzania_zespołami.Models;

namespace Aplikacja_webowa_do_zarządzania_zespołami.PartialModels
{
    public class CreateMessagePartial
    {
        public Message message { get; set; }
        public List<User>? usersList { get; set; }
        public string? messageUsers { get; set; }
    }
}
