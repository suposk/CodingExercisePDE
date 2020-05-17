using System;
using System.Collections.Generic;
using System.Text;

namespace CodingExercisePDE.Domain.ServiceBus
{
    public enum ServiceBusMessageType
    {
        Created,
        Posted,
    }

    public interface IMessagePayload
    {
        DateTime Created { get; set; }
        Guid Id { get; set; }
        int Number { get; set; }
        ServiceBusMessageType ServiceBusMessageType { get; set; }        
    }
}
