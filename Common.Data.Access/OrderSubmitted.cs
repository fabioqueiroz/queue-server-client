using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data.Access
{
    public class OrderSubmitted : IEvent
    {
        public Guid OrderId { get; set; }
        public decimal Value { get; set; }
    }
}
