using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DTO;
using ExamEdu.DTO.NotificationDTO;
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

        public NotifyController(IHubContext<NotifyHub> notifyHub)
        {
            _notifyHub = notifyHub;
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
            //await _cacheProvider.AddValueToKey<NotifyMessage>(notifyMessage.SendTo, notifyMessage);
            await _notifyHub.Clients.Group(notifyMessage.SendTo.ToLower()).SendAsync("ReceiveNotification", notifyMessage);
            return Ok(new ResponseDTO(204, "Successfully invoke"));
        }
    }
}