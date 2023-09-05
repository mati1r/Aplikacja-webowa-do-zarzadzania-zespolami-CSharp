using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class JoinGroupModel : PageModel
    {

        private readonly IGroupRepository _groupRepository;

        public JoinGroupModel(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            groupJoinList = new List<GroupJoinPartial>();
            groupQuitList = new List<GroupQuitPartial>();
        }

        public List<GroupJoinPartial> groupJoinList;
        public List<GroupQuitPartial> groupQuitList;
        public string data;
        public int? userId;
        public string username;

        [BindProperty]
        public int groupJoinId { get; set; }

        [BindProperty]
        public int groupQuitId { get; set; }

        [BindProperty]
        public Models.Group createGroup { get; set; }
        public string error;


        //On get and post methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                groupJoinList = _groupRepository.GetGroupsToJoin((int)userId);
                groupQuitList = _groupRepository.GetGroupsToQuit((int)userId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostJoin()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            //Check if group exists
            if(!_groupRepository.WhetherGroupExists(groupJoinId))
            {
                validationErrors.Add("Podana grupa nie istnieje");
                return new JsonResult(validationErrors);
            }

            if(_groupRepository.IsUserPartOfGroup(userId,groupJoinId))
            {
                validationErrors.Add("Użytkownik znajduje się już w tej grupie");
                return new JsonResult(validationErrors);
            }

            try
            {
                return _groupRepository.AddPendingUserToGroup((int)userId, groupJoinId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostQuit()
        {
            //Sprawdzić czy istnieje grupa z której użytkownik chce wyjść oraz czy jest on jej członkiem
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            //Check if user is part of a group
            if( !_groupRepository.IsUserPartOfGroup(userId, groupQuitId))
            {
                validationErrors.Add("Użytkownik nie znajduje sie w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if user is not a creator
            if(_groupRepository.IsUserAnCreator(userId, groupQuitId))
            {
                validationErrors.Add("Twórca grupy nie może z niej wyjść");
                return new JsonResult(validationErrors);
            }

            try
            {
                return _groupRepository.RemoveActiveUserFromGroup(groupQuitId, (int)userId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostCreate()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if (_groupRepository.IsGroupNameTaken(createGroup.name))
            {
                validationErrors.Add("Jest już grupa o tej nazwie");
                return new JsonResult(validationErrors);
            }

            //if there is no such group
            try
            {
                return _groupRepository.CreateGroup(createGroup, (int)userId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        //Partial methods
        public PartialViewResult OnGetGroupsJoinPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                groupJoinList = _groupRepository.GetGroupsToJoin((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialGroupsJoin", groupJoinList);
        }

        public PartialViewResult OnGetGroupsQuitPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                groupQuitList = _groupRepository.GetGroupsToQuit((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialGroupsQuit", groupQuitList);
        }

        //Async Methods
        public async Task<JsonResult> OnGetGroupJoinJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            return new JsonResult(await _groupRepository.GetGroupJoinAsync(id, userId));
        }

        public async Task<JsonResult> OnGetGroupQuitJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            return new JsonResult(await _groupRepository.GetGroupQuitAsync(id, userId));
        }
    }
}
