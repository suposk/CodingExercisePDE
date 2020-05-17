using CodingExercisePDE.Domain.ServiceBus;
using CodingExercisePDE.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodingExercisePDE.ServiceBusMessaging
{
    public class ServiceBusSender : IServiceBusSender
    {
        private readonly QueueClient _queueClientCreated;        
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusSender> _logger;
        private const string QUEUE_NAME = "numbergenerated";        

        public ServiceBusSender(IConfiguration configuration,
            ILogger<ServiceBusSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _queueClientCreated = new QueueClient(_configuration.GetConnectionString("ServiceBusConnectionString"), QUEUE_NAME);            
        }

        public async Task SendMessage(IMessagePayload payload)
        {
            string data = JsonConvert.SerializeObject(payload);
            Message message = new Message(Encoding.UTF8.GetBytes(data));

            _logger.LogDebug($"SendMessage To Queue: {payload}");

            if (payload.ServiceBusMessageType == ServiceBusMessageType.Created)
                await _queueClientCreated.SendAsync(message).ConfigureAwait(false);
        }
    }
}
