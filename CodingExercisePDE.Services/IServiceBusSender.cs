using CodingExercisePDE.Domain.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodingExercisePDE.Services
{
    public interface IServiceBusSender
    {
        Task SendMessage(IMessagePayload payload);
    }
}
