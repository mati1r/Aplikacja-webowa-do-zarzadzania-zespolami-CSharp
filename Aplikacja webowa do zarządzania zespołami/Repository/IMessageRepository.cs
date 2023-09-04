using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IMessageRepository
    {
        //Messages Page
        List<ReciveMessagePartial> GetRecivedMessages(int howManyRecords, int userId, int groupId);
        Message GetRecivedMessageContent(int messageId, int userId, int groupId);
        List<SendedMessagePartial> GetSendedMessages(int howManyRecords, int userId, int groupId);
        List<SendedMessagePartial> GetSendedMessageContent(int messageId, int userId, int groupId);
        JsonResult CreateMessage(Message message, List<int> reciversList, int userId, int groupId);

        //Owner and User Notices Page
        List<NoticePartial> GetNotice(int? groupId);
        Task<NoticePartial> GetNoticeAsync(int id, int? groupId);
        bool IsNotice(int id, int? groupId);

        //Owner Notices Page
        JsonResult DeleteNotice(Message notice, int? groupId);
        JsonResult CreateNotice(Message notice, int userId, int groupId);
        JsonResult EditNotice(Message notice, int userId, int groupId);

    }
}
