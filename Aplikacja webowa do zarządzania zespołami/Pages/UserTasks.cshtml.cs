﻿using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserTasksModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public UserTasksModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            tasksList = new List<Models.Tasks>();
        }


        public List<Models.Tasks> tasksList;
        public const string Key = "_userType";
        public string data;

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            tasksList = _dbContext.Tasks.ToList<Models.Tasks>();
        }

        public async Task<Models.Tasks> GetTaskAsync(int id)
        {
            return await _dbContext.Tasks.FirstOrDefaultAsync(p => p.task_id == id);
        }


        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            return new JsonResult(await GetTaskAsync(id));
        }

        [BindProperty(SupportsGet = true)]
        public int Action_Task_Id { get; set; }

        public IActionResult OnPostSend()
        {
            Console.WriteLine("Id =" + Action_Task_Id);
            // Wykonaj inne operacje lub przekierowanie
            return RedirectToPage("UserTasks");
        }
    }
}