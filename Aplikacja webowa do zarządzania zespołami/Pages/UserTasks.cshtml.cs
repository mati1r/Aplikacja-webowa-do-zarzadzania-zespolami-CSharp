using Aplikacja_webowa_do_zarządzania_zespołami.Models;
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
        }


        public List<Models.Task> TasksList;

        public void OnGet()
        {
            TasksList = _dbContext.Task.ToList<Models.Task>();
        }

        public async Task<Models.Task> GetTaskAsync(int id)
        {
            return await _dbContext.Task.FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            return new JsonResult(await GetTaskAsync(id));
        }
    }
}