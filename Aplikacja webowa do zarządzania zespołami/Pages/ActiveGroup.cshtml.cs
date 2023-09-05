using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class GroupsModel : PageModel
    {
        private readonly IGroupRepository _groupRepository;
        public GroupsModel(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            groupList = new List<ActiveGroupDTO>();
        }

        public List<ActiveGroupDTO> groupList;
        public string data;
        public int? userId;
        public int? groupId;
        public string username;
        public string activeGroup;

        [BindProperty(SupportsGet = true)]
        public int changeGroupId { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            //Read session data
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                groupList = _groupRepository.GetGroupsForActiveUser((int)userId);
                activeGroup = _groupRepository.GetGroupName((int)groupId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostChangeActiveGroup()
        {
            //I need to get values again this time for Post request
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            //Check if someone doesn't replaced id with some other id that is not in groupList
            if (_groupRepository.IsUserActiveMemberOfSelectedGroup(userId,changeGroupId))
            {
                //Check if user is an owner
                if(_groupRepository.IsUserAnOwnerOfSelectedGroup(userId,changeGroupId))
                {
                    HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), "Owner");
                }
                else
                {
                    HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), "User");
                }
                HttpContext.Session.SetInt32(ConstVariables.GetKeyValue(3), changeGroupId);
            }

            return RedirectToPage("ActiveGroup");
        }
    }
}
