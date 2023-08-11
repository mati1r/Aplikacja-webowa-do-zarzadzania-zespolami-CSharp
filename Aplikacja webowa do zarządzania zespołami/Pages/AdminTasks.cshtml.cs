using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class AdminTasksModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public AdminTasksModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            TasksList = new List<Models.Tasks>();
        }


        public List<Models.Tasks> TasksList;
        public const string Key = "_userType";
        public string data;

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            TasksList = _dbContext.Tasks.ToList<Models.Tasks>();
        }

        public async Task<Models.Tasks> GetTaskAsync(int id)
        {
            //Jeżeli ktos usunie w trakcie gdy mamy zaladowane dane do tabeli i klikniemy details to tu wystąpi błąd !!! OBMYŚLEĆ ROZWIĄZANIE
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
