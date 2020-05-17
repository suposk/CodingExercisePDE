using System;
using System.Collections.Generic;
using System.Text;

namespace CodingExercisePDE.Domain.ServiceBus
{
    public class MessagePayload : IMessagePayload
    {
        public Guid Id { get; set; }        
        public int Number { get; set; }
        public ServiceBusMessageType ServiceBusMessageType { get; set; }
        public DateTime Created { get; set; }        

        public override string ToString()
        {
            return $"Number {Number}, Id:{Id}";
        }
    }
}
