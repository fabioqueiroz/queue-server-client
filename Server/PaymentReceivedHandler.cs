using Common;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;

namespace Server.Api
{
    public class PaymentReceivedHandler : IHandleMessages<Payment>
    {
        private readonly ILog _log = LogManager.GetLogger<Payment>();

        public Task Handle(Payment payment, IMessageHandlerContext context)
        {
            _log.Info($"Received payment with Id {payment.Id}.");
            return Task.CompletedTask;
        }
    }
}
