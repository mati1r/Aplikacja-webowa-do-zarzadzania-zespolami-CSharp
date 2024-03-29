using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplikacja_webowa_do_zarządzania_zespołami.DAO;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly IMessageDAO _messageDAO;
        private readonly IUserDAO _userDAO;
        public MessagesModel(IMessageDAO messageDAO, IUserDAO userDAO)
        {
            _messageDAO = messageDAO;
            _userDAO = userDAO;
            reciveMessagesList = new List<ReciveMessagePartial>();
            sendedMessagesList = new List<SendedMessagePartial>();
            createMessagesPartial = new CreateMessagePartial();
            message = new Message();
        }

        public List<ReciveMessagePartial> reciveMessagesList;
        public List<SendedMessagePartial> sendedMessagesList;
        public Message message;
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;

        [BindProperty(SupportsGet = true)]
        public CreateMessagePartial createMessagesPartial { get; set; }


        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            //Check if user didn't deleted session (it causes function to throw exeptions
            try
            {
                reciveMessagesList = _messageDAO.GetRecivedMessages(10, 0, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostCreate()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if (createMessagesPartial.messageUsers == "" || createMessagesPartial.messageUsers == null)
            {
                validationErrors.Add("Nie podano poprawnego odbiorcy");
                return new JsonResult(validationErrors);
            }

            //Parse value from hidden input (selectize)
            string[] reciversArray = createMessagesPartial.messageUsers.Split(',');
            List<int> reciversList = new List<int>();

            foreach (string reciver in reciversArray)
            {
                if (int.TryParse(reciver, out int parsedReciver))
                {
                    reciversList.Add(parsedReciver);
                }
                else
                {
                    validationErrors.Add("Nie znaleziono odbiorcy");
                    return new JsonResult(validationErrors);
                }
            }

            List<int> usersIdList = _userDAO.GetIdOfActiveUsersInGroup(userId, groupId);

            //Check if values on the list are valid users from that group
            foreach (int receiverId in reciversList)
            {
                if (!usersIdList.Contains(receiverId))
                {
                    validationErrors.Add("Nie znaleziono odbiorcy");
                    return new JsonResult(validationErrors);
                }
            }

            //If we are here that means that passed recivers are correct and are in the group
            string error = MessageValidation.IsMessageValid(createMessagesPartial.message.topic);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            //At this point everything is correct so we can create a message
            _messageDAO.CreateMessage(createMessagesPartial.message, reciversList, (int)userId, (int)groupId);
            return new JsonResult("success");
        }

        //Partial methods
        public PartialViewResult OnGetReciveMessagesPartial(int howMany, int fromRecord)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                reciveMessagesList = _messageDAO.GetRecivedMessages(howMany, fromRecord, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialReciveMessagesView", reciveMessagesList);
        }

        public PartialViewResult OnGetReciveSearchMessagesPartial(string condition)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                reciveMessagesList = _messageDAO.GetRecivedSearchMessages(condition, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialReciveMessagesView", reciveMessagesList);
        }

        public PartialViewResult OnGetReciveMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                message = _messageDAO.GetRecivedMessageContent(id, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }
            return Partial("Partials/_PartialReciveMessage", message);
        }

        public PartialViewResult OnGetSendedMessagesPartial(int howMany, int fromRecord)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            //Get list of messages and nickname of one person that recived it (Group by and select statment)
            try
            {
                sendedMessagesList = _messageDAO.GetSendedMessages(howMany, fromRecord, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }


            return Partial("Partials/_PartialSendedMessagesView", sendedMessagesList);
        }

        public PartialViewResult OnGetSendedSearchMessagesPartial(string condition)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            //Get list of messages and nickname of one person that recived it (Group by and select statment)
            try
            {
                sendedMessagesList = _messageDAO.GetSendedSearchMessages(condition, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }


            return Partial("Partials/_PartialSendedMessagesView", sendedMessagesList);
        }

        public PartialViewResult OnGetSendedMessage(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            try
            {
                sendedMessagesList = _messageDAO.GetSendedMessageContent(id, (int)userId, (int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialSendedMessage", sendedMessagesList);
        }

        public PartialViewResult OnGetCreateMessage()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            createMessagesPartial.usersList = _userDAO.GetActiveUsersInGroupBesidesYourself(userId, groupId);

            return Partial("Partials/_PartialCreateMessage", createMessagesPartial);
        }
    }
}
