using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Pages.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class OwnerNoticesModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public OwnerNoticesModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            noticesList = new List<NoticePartial>();
        }

        public List<NoticePartial> noticesList;
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty(SupportsGet = true)]
        public Message notice { get; set; }

        private List<NoticePartial> GetNotice(int? groupId)
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

            noticesList = GetNotice(groupId);
        }

        public IActionResult OnPostDelete()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if message exists, if its in the group and is a notice
            if (_dbContext.Messages.Count(m => m.message_id == notice.message_id && m.groups_group_id == groupId && m.notice == true) == 0)
            {
                validationErrors.Add("Podane ogłoszenie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            _dbContext.Remove(notice);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }

        public IActionResult OnPostAdd()
        {
            string error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            //Check notice topic formating
            error = MessageValidation.IsMessageValid(notice.topic);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            notice.notice = true;
            notice.sender_id = (int)userId;
            notice.groups_group_id = (int)groupId;
            notice.send_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            _dbContext.Add(notice);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public IActionResult OnPostEdit()
        {
            string error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if message exists, if its in the group and is a notice
            if (_dbContext.Messages.Count(m => m.message_id == notice.message_id && m.groups_group_id == groupId && m.notice == true) == 0)
            {
                validationErrors.Add("Podane ogłoszenie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            //Check notice topic formating
            error = MessageValidation.IsMessageValid(notice.topic);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            notice.groups_group_id = (int)groupId;
            notice.send_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            notice.sender_id = (int)userId;
            notice.notice = true;
            _dbContext.Update(notice);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        //Partial methods
        public PartialViewResult OnGetLoadNotices()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                noticesList = GetNotice(groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialNoticesView", noticesList);
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
