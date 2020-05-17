using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodingExercisePDE.ServiceBusMessaging
{
    public interface IServiceBusConsumer
    {
        void RegisterOnMessageHandlerAndReceiveMessages(object param = null);
        Task CloseQueueAsync();
    }
}
