using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DatabaseContext _dbContext;
        public MessageRepository(DatabaseContext dbContext) 
        { 
            _dbContext = dbContext;
        }

        public JsonResult CreateMessage(Message message, List<int> reciversList, int userId, int groupId)
        {
            message.groups_group_id = groupId;
            message.send_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            message.sender_id = userId;
            message.notice = false;
            _dbContext.Add(message);
            _dbContext.SaveChanges();

            Message_User messageUser = new Message_User();

            foreach (int receiverId in reciversList)
            {
                messageUser.users_user_id = receiverId;
                messageUser.messages_message_id = message.message_id;
                _dbContext.Add(messageUser);
                _dbContext.SaveChanges();
            }
            return new JsonResult("success");
        }

        public Message GetRecivedMessageContent(int messageId, int userId, int groupId)
        {
            //Get message and validate if user didn't changed id to some out of his scope or active group
            if (_dbContext.Messages.Any(m => m.Messages_Users.Any(mu => mu.users_user_id == userId)
                                        && m.groups_group_id == groupId && m.notice == false && m.message_id == messageId))
            {
                return _dbContext.Messages.Where(m => m.message_id == messageId).First();
            }
            Message falseMessage = new Message();
            falseMessage.content = "Błąd, widomość o tym id nie istnieje";
            return falseMessage;
        }

        public List<ReciveMessagePartial> GetRecivedMessages(int howManyRecords, int userId, int groupId)
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

        public List<SendedMessagePartial> GetSendedMessageContent(int messageId, int userId, int groupId)
        {
            List<SendedMessagePartial> messagesList = new List<SendedMessagePartial>();
            if (_dbContext.Messages.Any(m => m.sender_id == userId && m.groups_group_id == groupId
                                         && m.notice == false && m.message_id == messageId))
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

        public List<SendedMessagePartial> GetSendedMessages(int howManyRecords, int userId, int groupId)
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
    }
}
