using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data.Access
{
    public class OrderAccepted : IMessage
    {
        public Guid OrderId { get; set; }
    }
}
