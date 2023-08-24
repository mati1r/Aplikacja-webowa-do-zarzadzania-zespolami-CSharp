using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public MessagesModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            reciveMessagesList = new List<ReciveMessageView>();
            sendedMessagesList = new List<SendedMessageView>();
            createMessagesView = new CreateMessageView();
            message = new Models.Message();
        }

        //Subclass of messages that takes sender_name insted of sender_id so it can be shown to a user
        public class ReciveMessageView
        {
            public int message_id {  get; set; }
            public string topic { get; set; }
            public string content { get; set; }
            public DateTime send_date { get; set; }
            public string sender_name { get; set; }
        }

        public class SendedMessageView
        {
            public int message_id { get; set; }
            public string topic { get; set; }
            public string content { get; set; }
            public DateTime send_date { get; set; }
            public string reciver_name { get; set; }
        }

        public class CreateMessageView
        {
            public Models.Message message { get; set; }
            public List<Models.User>? usersList { get; set; }
            public string? messageUsers { get; set; }
        }

        public List<ReciveMessageView> reciveMessagesList;
        public List<SendedMessageView> sendedMessagesList;
        public Message message;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty(SupportsGet = true)]
        public CreateMessageView createMessagesView { get; set; }

        private List<ReciveMessageView> GetMessages(int howManyRecords, int userId, int groupId)
        {
            return _dbContext.Messages
                .Include(m => m.Users) //Include Users table by FK
                .Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId) && m.groups_group_id == groupId) //Get elements that met the requaierments
                .OrderByDescending(m => m.send_date)
                .Take(howManyRecords)
                .Select(m => new ReciveMessageView
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    sender_name = m.Users.username
                })
                .ToList();
        }

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            reciveMessagesList = GetMessages(10, (int)userId, (int)groupId);
        }

        public PartialViewResult OnGetReciveMessagesView(int howMany)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            reciveMessagesList = GetMessages(howMany, (int)userId, (int)groupId);

            return Partial("Partials/_PartialReciveMessagesView", reciveMessagesList);
        }

        public PartialViewResult OnGetSendedMessagesView(int howMany)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            //Get list of messages and nickname of one person that recived it (Group by and select statment)
            sendedMessagesList = _dbContext.Messages
                .Where(m => m.sender_id == userId && m.groups_group_id == groupId)
                .OrderByDescending(m => m.send_date)
                .SelectMany(m => m.Messages_Users, (m, mu) => new SendedMessageView
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    reciver_name = mu.Users.username
                })
                .GroupBy(m => m.message_id)
                .Select(m => m.First())
                .Take(howMany)
                .ToList();


            return Partial("Partials/_PartialSendedMessagesView", sendedMessagesList);
        }


        //Get message and validate if user didn't changed id to some out of his scope or active group
        private Message GetReciveMessageContent(int messageId, int userId, int groupId)
        {
            if(_dbContext.Messages.Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId) && m.groups_group_id == groupId && m.message_id == messageId).Count() > 0)
            {
                return _dbContext.Messages.Where(m => m.message_id == messageId).First();
            }
            Message falseMessage = new Message();
            falseMessage.content = "Błąd, widomość o tym id nie istnieje";
            return falseMessage;
        }

        public PartialViewResult OnGetReciveMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            message = GetReciveMessageContent(id, (int)userId, (int)groupId);
            return Partial("Partials/_PartialReciveMessage", message);
        }

        private List<SendedMessageView> GetSendedMessageContent(int messageId, int userId, int groupId)
        {
            List<SendedMessageView> messagesList = new List<SendedMessageView>();
            if (_dbContext.Messages.Where(m => m.sender_id == userId && m.groups_group_id == groupId && m.message_id == messageId).Count() > 0)
            {
                messagesList = _dbContext.Messages
                    .Where(m => m.message_id == messageId)
                    .SelectMany(m => m.Messages_Users, (m, mu) => new SendedMessageView
                    {
                        message_id = m.message_id,
                        topic = m.topic,
                        content = m.content,
                        send_date = m.send_date,
                        reciver_name = mu.Users.username
                    })
                    .ToList();
            }
            else
            {
                messagesList.Add(new SendedMessageView { message_id = messageId, topic = "", content = "Błąd, widomość o tym id nie istnieje", send_date = DateTime.Now, reciver_name = "" });
            }

            return messagesList;
        }

        public PartialViewResult OnGetSendedMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            sendedMessagesList = GetSendedMessageContent(id, (int)userId, (int)groupId);

            return Partial("Partials/_PartialSendedMessage", sendedMessagesList);
        }
        public PartialViewResult OnGetCreateMessage()
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            createMessagesView.usersList = _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "aktywny"))
                .ToList();

            return Partial("Partials/_PartialCreateMessage", createMessagesView);
        }

        public IActionResult OnPostCreate()
        {
            data = HttpContext.Session.GetString(Key);
            List<string> validationErrors = new List<string>();
            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                foreach(var item in modelStateValidationErrors)
                {
                    Console.WriteLine(item.Key);
                    foreach(var t in item.Value)
                    {
                        Console.WriteLine(t);
                    }
                }

                return new JsonResult(modelStateValidationErrors);
            }

            //WALIDACJA DZIAŁA DLA TEMATU I WIADOMOŚCI, NIE DZIALA DLA ODBIORCY BO ODRAZU Z JAKIEGOŚ POWODU WYŚWIETLAŁ SIĘ BŁAD 
            //DLA TEMATU I WIADOMOSCI SPRAWDZIC FORMATOWANIE A DLA ODBIORCY SPRAWDZIC CZY COS PRZYCHODZI, NASTEPNIE PRZEKONWERTOWAC TO NA LISTE INTÓW
            //PRZYCHODZIĆ BĘDĄ np. "1,4,5" TAK WIĘC WARTOŚCI SĄ PO PROSTU ODZIELONE PRZECINKIEM, NALEŻY TO PRZEROBIĆ NA LISTE I SPARSOWAĆ NA INTY
            //NASTĘPNIE SPRAWDZIĆ CZY TE WARTOŚCI SĄ NA LISCIE OSOB W GRUPIE, JEZELI TAK TO JEST GIT
            Console.WriteLine(createMessagesView.message.content);
            return new JsonResult("success");
        }
    }
}
