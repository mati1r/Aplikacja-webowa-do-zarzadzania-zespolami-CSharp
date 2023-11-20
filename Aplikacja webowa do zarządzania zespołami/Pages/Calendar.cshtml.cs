using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Aplikacja_webowa_do_zarządzania_zespołami.DAO;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly ITaskDAO _taskDAO;

        public CalendarModel(ITaskDAO taskDAO)
        {
            _taskDAO = taskDAO;
            tasksList = new List<Models.Task>();
        }

        public List<Models.Task> tasksList;
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;


        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));
        }

        //Async methods
        public async Task<JsonResult> OnGetEventsAsync()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            List<CalendarEventsDTO> events = await _taskDAO.GetCalendarEventsAsync(userId, groupId);

            return new JsonResult(events);
        }

        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _taskDAO.GetTaskAsync(id, userId, groupId));
        }
    }
}
