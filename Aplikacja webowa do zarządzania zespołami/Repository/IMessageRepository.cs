using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IMessageRepository
    {
        //Message Page
        List<ReciveMessagePartial> GetRecivedMessages(int howManyRecords, int userId, int groupId);
        Message GetRecivedMessageContent(int messageId, int userId, int groupId);
        List<SendedMessagePartial> GetSendedMessages(int howManyRecords, int userId, int groupId);
        List<SendedMessagePartial> GetSendedMessageContent(int messageId, int userId, int groupId);
        JsonResult CreateMessage(Message message, List<int> reciversList, int userId, int groupId);
    }
}
