using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
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
            reciveMessagesList = new List<ReciveMessagePartial>();
            sendedMessagesList = new List<SendedMessagePartial>();
            createMessagesPartial = new CreateMessagePartial();
            message = new Models.Message();
        }


        public List<ReciveMessagePartial> reciveMessagesList;
        public List<SendedMessagePartial> sendedMessagesList;
        public Message message;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty(SupportsGet = true)]
        public CreateMessagePartial createMessagesPartial { get; set; }

        //Recived message private methods
        private List<ReciveMessagePartial> GetRecivedMessages(int howManyRecords, int userId, int groupId)
        {
            return _dbContext.Messages
                .Include(m => m.Users) //Include Users table by FK
                .Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId) && m.groups_group_id == groupId && m.notice == false) //Get elements that met the requaierments
                .OrderByDescending(m => m.send_date)
                .Take(howManyRecords)
                .Select(m => new ReciveMessagePartial
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    sender_name = m.Users.username
                })
                .ToList();
        }

        private Message GetRecivedMessageContent(int messageId, int userId, int groupId)
        {
            //Get message and validate if user didn't changed id to some out of his scope or active group
            if (_dbContext.Messages.Where(m => m.Messages_Users.Any(mu => mu.users_user_id == userId)
                                        && m.groups_group_id == groupId && m.notice == false && m.message_id == messageId).Count() > 0)
            {
                return _dbContext.Messages.Where(m => m.message_id == messageId).First();
            }
            Message falseMessage = new Message();
            falseMessage.content = "Błąd, widomość o tym id nie istnieje";
            return falseMessage;
        }

        //Sended message private methods
        private List<SendedMessagePartial> GetSendedMessages(int howManyRecords, int userId, int groupId)
        {
            return _dbContext.Messages
                .Where(m => m.sender_id == userId && m.groups_group_id == groupId && m.notice == false)
                .SelectMany(m => m.Messages_Users, (m, mu) => new SendedMessagePartial
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    reciver_name = mu.Users.username
                })
                .GroupBy(g => g.message_id)
                .Select(g => g.OrderByDescending(m => m.send_date).First())
                .AsEnumerable() //Changing execution mode to the application level
                .OrderByDescending(m => m.send_date)
                .Take(howManyRecords)
                .ToList();
        }

        private List<SendedMessagePartial> GetSendedMessageContent(int messageId, int userId, int groupId)
        {
            List<SendedMessagePartial> messagesList = new List<SendedMessagePartial>();
            if (_dbContext.Messages.Where(m => m.sender_id == userId && m.groups_group_id == groupId
                                         && m.notice == false && m.message_id == messageId).Count() > 0)
            {
                messagesList = _dbContext.Messages
                    .Where(m => m.message_id == messageId)
                    .SelectMany(m => m.Messages_Users, (m, mu) => new SendedMessagePartial
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
                messagesList.Add(new SendedMessagePartial
                {
                    message_id = messageId,
                    topic = "",
                    content = "Błąd, widomość o tym id nie istnieje",
                    send_date = DateTime.Now,
                    reciver_name = ""
                });
            }

            return messagesList;
        }

        //Create message private methods
        private void CreateMessage(Message message, List<int> reciversList)
        {
            message.groups_group_id = (int)groupId;
            message.send_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            message.sender_id = (int)userId;
            message.notice = false;
            _dbContext.Add(message);
            _dbContext.SaveChanges();

            Message_User messageUser = new Message_User();

            foreach (int receiverId in reciversList)
            {
                messageUser.users_user_id = receiverId;
                messageUser.messages_message_id = createMessagesPartial.message.message_id;
                _dbContext.Add(messageUser);
                _dbContext.SaveChanges();
            }
        }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            //Check if user didn't deleted session (it causes function to throw exeptions (even tho it should be able to accept null as userId and groupId))
            try
            {
                reciveMessagesList = GetRecivedMessages(10, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostCreate()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            List<string> validationErrors = new List<string>();
            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if (createMessagesPartial.messageUsers == "" || createMessagesPartial.messageUsers == null)
            {
                validationErrors.Add("Nie podano poprawnego odbiorcy");
                return new JsonResult(validationErrors);
            }

            //Parse value from hidden input (selectize)
            string[] reciversArray = createMessagesPartial.messageUsers.Split(',');
            List<int> reciversList = new List<int>();

            foreach (string reciver in reciversArray)
            {
                if (int.TryParse(reciver, out int parsedReciver))
                {
                    reciversList.Add(parsedReciver);
                }
                else
                {
                    validationErrors.Add("Nie znaleziono odbiorcy");
                    return new JsonResult(validationErrors);
                }
            }

            //Get values of users that are in the group
            List<int> usersIdList = _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .Select(g => g.user_id).ToList();


            //Check if values on the list are valid users from that group
            foreach (int receiverId in reciversList)
            {
                if (!usersIdList.Contains(receiverId))
                {
                    validationErrors.Add("Nie znaleziono odbiorcy");
                    return new JsonResult(validationErrors);
                }
            }

            //If we are here that means that passed recivers are correct and are in the group
            string error = MessageValidation.IsMessageValid(createMessagesPartial.message.topic);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            //At this point everything is correct so we can create a message
            CreateMessage(createMessagesPartial.message, reciversList);
            return new JsonResult("success");
        }

        //Partial methods
        public PartialViewResult OnGetReciveMessagesPartial(int howMany)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            try
            {
                reciveMessagesList = GetRecivedMessages(howMany, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialReciveMessagesView", reciveMessagesList);
        }

        public PartialViewResult OnGetReciveMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            try
            {
                message = GetRecivedMessageContent(id, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }
            return Partial("Partials/_PartialReciveMessage", message);
        }

        public PartialViewResult OnGetSendedMessagesPartial(int howMany)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            //Get list of messages and nickname of one person that recived it (Group by and select statment)
            try
            {
                sendedMessagesList = GetSendedMessages(howMany, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }


            return Partial("Partials/_PartialSendedMessagesView", sendedMessagesList);
        }

        public PartialViewResult OnGetSendedMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            try
            {
                sendedMessagesList = GetSendedMessageContent(id, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialSendedMessage", sendedMessagesList);
        }
        public PartialViewResult OnGetCreateMessage()
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            createMessagesPartial.usersList = _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .ToList();

            return Partial("Partials/_PartialCreateMessage", createMessagesPartial);
        }
    }
}
