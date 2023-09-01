using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Pages.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserNoticesModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public UserNoticesModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            noticesList = new List<NoticePartial>();
        }

        public List<NoticePartial> noticesList;
        public string data;
        public int? userId;
        public int? groupId;

        private List<NoticePartial> GetNotice(int? userId, int? groupId)
        {
            return _dbContext.Messages
                .Include(m => m.Users) //Include Users table by FK
                .Where(m => m.groups_group_id == groupId && m.notice == true) //Get elements that meet the requaierments
                .OrderByDescending(m => m.send_date)
                .Select(m => new NoticePartial
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    sender_name = m.Users.username
                })
                .ToList();
        }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            noticesList = GetNotice(userId, groupId);  
        }

        //Async methods
        public async Task<NoticePartial> GetNoticeAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            //Check if user didn't changed id to an id out of his scope or to an message insted of notice
            if (_dbContext.Messages.Count(m => m.message_id == id && m.groups_group_id == groupId && m.notice == true) > 0)
            {
                return await _dbContext.Messages
                .Include(m => m.Users)
                .Where(m => m.message_id == id)
                .Select(m => new NoticePartial
                {
                    message_id = m.message_id,
                    topic = m.topic,
                    content = m.content,
                    send_date = m.send_date,
                    sender_name = m.Users.username
                }).FirstAsync(m => m.message_id == id);
            }
            else
            {
                NoticePartial emptyNotice = new NoticePartial();
                emptyNotice.send_date = DateTime.Now;
                emptyNotice.message_id = 0;
                emptyNotice.content = "Błąd nie znaleziono ogłoszenia";
                emptyNotice.sender_name = "Błąd";
                emptyNotice.topic = "Błąd";
                return emptyNotice;
            }
        }


        public async Task<JsonResult> OnGetNoticeJsonAsync(int id)
        {
            return new JsonResult(await GetNoticeAsync(id));
        }
    }
}
