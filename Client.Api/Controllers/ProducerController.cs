using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Client.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly ILogger<ProducerController> _logger;
        public ProducerController(IMessageSession messageSession, ILogger<ProducerController> logger)
        {
            _messageSession = messageSession;
            _logger = logger;
        }

        /// <summary>
        /// Records a new payment.
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [ProducesResponseType(typeof(OkObjectResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Exception), (int)HttpStatusCode.InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> SendPaymentMessage([FromBody] Payment payment)
        {
            if (payment == null) return BadRequest("No payment info provided.");

            try
            {
                await _messageSession.Send("transactions", payment).ConfigureAwait(false);
                _logger.LogInformation($"Payment Id {payment.Id} received");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to send payment; {ex.Message}");
                throw new Exception(ex.Message);
            }

            return Ok("Payment message sent.");
        }
    }
}
