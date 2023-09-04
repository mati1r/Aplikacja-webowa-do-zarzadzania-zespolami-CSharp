using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class OwnerNoticesModel : PageModel
    {
        private readonly IMessageRepository _messageRepository;
        public OwnerNoticesModel(DatabaseContext dbContext, IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
            noticesList = new List<NoticePartial>();
            notice = new Message();
        }

        public List<NoticePartial> noticesList;
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;

        [BindProperty(SupportsGet = true)]
        public Message notice { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            noticesList = _messageRepository.GetNotice(groupId);
        }

        public IActionResult OnPostDelete()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            return _messageRepository.DeleteNotice(notice, groupId);
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

            //If user deleted session (it is like that to do not let pass nulls to db)
            try
            {
                return _messageRepository.CreateNotice(notice, (int)userId, (int)groupId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostEdit()
        {
            string error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            if (!_messageRepository.IsNotice(notice.message_id, groupId))
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

            error = MessageValidation.IsMessageValid(notice.topic);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            try
            {
                return _messageRepository.EditNotice(notice, (int)userId, (int)groupId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        //Partial methods
        public PartialViewResult OnGetLoadNotices()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                noticesList = _messageRepository.GetNotice(groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialNoticesView", noticesList);
        }

        //Async methods
        public async Task<JsonResult> OnGetNoticeJsonAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _messageRepository.GetNoticeAsync(id, groupId));
        }
    }
}
