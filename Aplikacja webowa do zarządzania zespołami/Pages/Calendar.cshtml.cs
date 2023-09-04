using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class CalendarModel : PageModel
    {

        private readonly DatabaseContext _dbContext;
        private readonly ITaskRepository _taskRepository;

        public CalendarModel(DatabaseContext dbContext, ITaskRepository taskRepository)
        {
            _dbContext = dbContext;
            _taskRepository = taskRepository;
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

            List<CalendarEventsDTO> events = await _taskRepository.GetCalendarEventsAsync(userId, groupId);

            return new JsonResult(events);
        }
    }
}
