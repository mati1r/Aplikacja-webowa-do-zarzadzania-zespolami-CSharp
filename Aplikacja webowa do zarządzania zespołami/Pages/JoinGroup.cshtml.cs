using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aplikacja_webowa_do_zarządzania_zespołami.DAO;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class JoinGroupModel : PageModel
    {

        private readonly IGroupDAO _groupDAO;

        public JoinGroupModel(IGroupDAO groupDAO)
        {
            _groupDAO = groupDAO;
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
                groupJoinList = _groupDAO.GetGroupsToJoin((int)userId);
                groupQuitList = _groupDAO.GetGroupsToQuit((int)userId);
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
            if(!_groupDAO.WhetherGroupExists(groupJoinId))
            {
                validationErrors.Add("Podana grupa nie istnieje");
                return new JsonResult(validationErrors);
            }

            if(_groupDAO.IsUserPartOfGroup(userId,groupJoinId))
            {
                validationErrors.Add("Użytkownik znajduje się już w tej grupie");
                return new JsonResult(validationErrors);
            }

            try
            {
                _groupDAO.AddPendingUserToGroup((int)userId, groupJoinId);
                return new JsonResult("success");
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
            if( !_groupDAO.IsUserPartOfGroup(userId, groupQuitId))
            {
                validationErrors.Add("Użytkownik nie znajduje sie w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if user is not a creator
            if(_groupDAO.IsUserAnCreator(userId, groupQuitId))
            {
                validationErrors.Add("Twórca grupy nie może z niej wyjść");
                return new JsonResult(validationErrors);
            }

            try
            {
                _groupDAO.RemoveActiveUserFromGroup(groupQuitId, (int)userId);
                return new JsonResult("success");
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

            if (_groupDAO.IsGroupNameTaken(createGroup.name))
            {
                validationErrors.Add("Jest już grupa o tej nazwie");
                return new JsonResult(validationErrors);
            }

            //if there is no such group
            try
            {
                _groupDAO.CreateGroup(createGroup, (int)userId);
                return new JsonResult("success");
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
                groupJoinList = _groupDAO.GetGroupsToJoin((int)userId);
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
                groupQuitList = _groupDAO.GetGroupsToQuit((int)userId);
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
            return new JsonResult(await _groupDAO.GetGroupJoinAsync(id, userId));
        }

        public async Task<JsonResult> OnGetGroupQuitJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            return new JsonResult(await _groupDAO.GetGroupQuitAsync(id, userId));
        }
    }
}
