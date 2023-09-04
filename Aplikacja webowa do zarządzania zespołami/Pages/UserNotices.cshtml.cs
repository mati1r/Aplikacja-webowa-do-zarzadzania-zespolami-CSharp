using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserNoticesModel : PageModel
    {
        private readonly IMessageRepository _messageRepository;
        public UserNoticesModel(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
            noticesList = new List<NoticePartial>();
        }

        public List<NoticePartial> noticesList;
        public string data;
        public int? groupId;
        public string? username;

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            noticesList = _messageRepository.GetNotice(groupId);  
        }

        //Async methods

        public async Task<JsonResult> OnGetNoticeJsonAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _messageRepository.GetNoticeAsync(id, groupId));
        }
    }
}
