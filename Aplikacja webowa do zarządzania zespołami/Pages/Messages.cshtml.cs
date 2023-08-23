using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public MessagesModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            messagesList = new List<MessageView>();
            message = new Models.Message();
        }

        //Subclass of messages that takes sender_name insted of sender_id so it can be shown to a user
        public class MessageView
        {
            public int message_id {  get; set; }
            public string topic { get; set; }
            public string content { get; set; }
            public DateTime send_date { get; set; }
            public string sender_name { get; set; }
        }

        public List<MessageView> messagesList;
        public Message message;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        private List<MessageView> GetMessages(int howManyRecords, int userId, int groupId)
        {
            return _dbContext.Messages
                .Include(m => m.Users) //Include Users table by FK
                .Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId) && m.groups_group_id == groupId) //Get elements that met the requaierments
                .OrderByDescending(m => m.send_date)
                .Take(howManyRecords)
                .Select(m => new MessageView
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    sender_name = m.Users.username // Pobierz nazwę nadawcy
                })
                .ToList();
        }

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            messagesList = GetMessages(10, (int)userId, (int)groupId);
        }

        public PartialViewResult OnGetMessagesView(int howMany)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            messagesList = GetMessages(howMany, (int)userId, (int)groupId);

            return Partial("Partials/_PartialMessagesView", messagesList);
        }


        //Get message and validate if user didn't changed id to some out of his scope or active group
        private Message GetMessageContent(int messageId, int userId, int groupId)
        {
            if(_dbContext.Messages.Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId) && m.groups_group_id == groupId && m.message_id == messageId).Count() > 0)
            {
                return _dbContext.Messages.Where(m => m.message_id == messageId).First();
            }
            Message falseMessage = new Message();
            falseMessage.content = "Błąd, widomość o tym id nie istnieje";
            return falseMessage;
        }

        public PartialViewResult OnGetMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            message = GetMessageContent(id, (int)userId, (int)groupId);
            return Partial("Partials/_PartialMessage", message);
        }
    }
}
