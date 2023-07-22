using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages.TestPages
{
    public class CalendarModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public CalendarModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Models.Calendar> pList;
        public void OnGet()
        {
            pList = _dbContext.Calendar.ToList<Models.Calendar>();
        }

        public async Task<JsonResult> OnGetEventsAsync()
        {
            var events = await _dbContext.Calendar
                .Select(e => new
                {
                    title = e.Title,
                    start = e.StartDate.ToString("yyyy-MM-dd"),
                    end = e.EndDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return new JsonResult(events);
        }
    }
}
