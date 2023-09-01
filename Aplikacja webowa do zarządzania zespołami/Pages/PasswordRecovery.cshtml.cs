using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Text;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class PasswordRecoveryModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public PasswordRecoveryModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public string email { get; set; }

        private string PasswordGenerator(int howLong)
        {
            //Using string builder cause it is much faster that appending to string
            const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder password = new StringBuilder();
            Random pickNumber = new Random();
            for(int i = 0; i < howLong; i++)
            {
                password.Append(characters[pickNumber.Next(characters.Length)]);
            }
            return password.ToString();
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostRecover()
        {
            List<string> validationErrors = new List<string>();

            if(!_dbContext.Users.Any(u => u.e_mail == email))
            {
                validationErrors.Add("Nie istnieje konto powiązane z tym adresem e-mail");
                return new JsonResult(validationErrors);
            }

            string newPassword = PasswordGenerator(16);

            SmtpClient client = new SmtpClient();
            
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("AWDZZmail@gmail.com", "puqonsvjixkcpmwp");

            MailMessage message = new MailMessage(
                from: new MailAddress("AWDZZmail@gmail.com", "AWDZZ"),
                to: new MailAddress(email, "Odbirca")
                );

            message.Subject = "Nowe hasło do aplikacji AWDZZ";
            message.Body = "Oto twoje nowe hasło:\n" + newPassword + "\n\nHasło możesz zmienić w menu profile w aplikacji";
            try
            {
                client.Send(message);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd spróbuj ponowinie później");
                return new JsonResult(validationErrors);
            }

            //If we are here that means that message was send
            User user = new User();
            user = _dbContext.Users.Where(u => u.e_mail == email).First();
            user.salt = Hash.GenerateSalt(16);
            user.password = Hash.HashPassword(newPassword, user.salt);

            _dbContext.Update(user);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

    }
}
