using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.MessagesModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class NoticesModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public NoticesModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            noticesList = new List<NoticeView>();
        }

        public class NoticeView
        {
            public int message_id { get; set; }
            public string topic { get; set; }
            public string content { get; set; }
            public DateTime send_date { get; set; }
            public string sender_name { get; set; }
        }

        public List<NoticeView> noticesList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        private List<NoticeView> GetNotice(int? userId, int? groupId)
        {
            return _dbContext.Messages
                .Include(m => m.Users) //Include Users table by FK
                .Where(m => m.groups_group_id == groupId && m.notice == true) //Get elements that met the requaierments
                .OrderByDescending(m => m.send_date)
                .Select(m => new NoticeView
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

            noticesList = GetNotice(userId, groupId);  
        }
    }
}
