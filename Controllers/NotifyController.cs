using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Services.Cache;
using examedu.Services.Classes;
using ExamEdu.DTO;
using ExamEdu.DTO.NotificationDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using ExamEdu.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace examedu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotifyController : ControllerBase
    {
     
        private readonly IHubContext<NotifyHub> _notifyHub;
        private readonly ICacheProvider _cacheProvider;

        private readonly IClassService _classService;
      

        public NotifyController(IHubContext<NotifyHub> notifyHub, ICacheProvider cacheProvider,
            IClassService classService)
        {
            _classService = classService;
            _notifyHub = notifyHub;
            _cacheProvider = cacheProvider;
        }

        
        /// <summary>
        /// invoke function ReceiveNotification and send data to trainee
        /// </summary>
        /// <param name="notifyMessage">user: email of admin / content: content / sendTo: trainee's email</param>
        /// <returns>400: cannot find trainee's email / 204: send success</returns>
        [HttpPost("trainee")]
        public async Task<ActionResult> SendTraineeNotification([FromBody] NotifyMessage notifyMessage)
        {
            // if(_traineeService.GetTraineeByEmail(notifyMessage.sendTo) == null )
            // {
            //     return BadRequest(new ResponseDTO(400, "Can not find trainee's email"));
            // }
            notifyMessage.SendTo = notifyMessage.SendTo.ToLower();
            notifyMessage.User = notifyMessage.User.ToLower();
            await _cacheProvider.AddValueToKey<NotifyMessage>(notifyMessage.SendTo, notifyMessage);
            await _notifyHub.Clients.Group(notifyMessage.SendTo.ToLower()).SendAsync("ReceiveNotification", notifyMessage);
            return Ok(new ResponseDTO(204, "Successfully invoke"));
        }
        /// <summary>
        /// invoke function ReceiveHistory and send data 
        /// </summary>
        /// <param name="email">email of user want to recieveHistory</param>
        /// <returns>404: email not found / 204: send success</returns>
        [HttpGet("history")]
        public async Task<ActionResult> SendHistory([FromQuery] string email, [FromQuery] PaginationParameter paginationParameter)
        {
            email = email.ToLower();
            List<NotifyMessage> history = new List<NotifyMessage>();
            history = await _cacheProvider.GetFromCache<List<NotifyMessage>>(email);
            if (history == null)
            {
                return NotFound(new ResponseDTO(404, "history not found!"));
            }

            history.Sort((x, y) => y.CreatedAt.CompareTo(x.CreatedAt));

            history = history.Where(t =>
                t.User.Contains(paginationParameter.SearchName.ToLower()) ||
                t.SendTo.Contains(paginationParameter.SearchName.ToLower())).Select(t => t).ToList();

            return Ok(new PaginationResponse<IEnumerable<NotifyMessage>>(history.Count(), PaginationHelper.GetPage(history, paginationParameter)));
        }
        [HttpPost("setSeen")]
        public async Task<ActionResult> SetSeen([FromBody] string email)
        {
            if (email == null)
            {
                return BadRequest(new ResponseDTO(404, "email not found"));
            }
            email = email.ToLower();
            List<NotifyMessage> history = new List<NotifyMessage>();
            history = await _cacheProvider.GetFromCache<List<NotifyMessage>>(email);
            if (history == null)
            {
                return BadRequest(new ResponseDTO(404, "history not found"));
            }
            foreach (var notify in history)
            {
                notify.IsSeen = true;
            }
            await _cacheProvider.SetCache<List<NotifyMessage>>(email, history);

            return Ok(new ResponseDTO(204, "Successfully setSeen"));
        }
    }
}