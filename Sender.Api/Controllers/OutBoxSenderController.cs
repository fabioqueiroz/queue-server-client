using Common.Data.Access;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace Sender.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OutBoxSenderController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly ILogger<OutBoxSenderController> _logger;

        public OutBoxSenderController(ILogger<OutBoxSenderController> logger, IMessageSession messageSession)
        {
            _logger = logger;
            _messageSession = messageSession;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var random = new Random();

            var orderSubmitted = new OrderSubmitted
            {
                OrderId = Guid.NewGuid(),
                Value = random.Next(100)
            };

            await _messageSession.Publish(orderSubmitted).ConfigureAwait(false);

            return Ok();
        }
    }
}
