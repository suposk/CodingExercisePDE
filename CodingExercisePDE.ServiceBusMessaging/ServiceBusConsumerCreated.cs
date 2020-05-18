using CodingExercisePDE.Domain;
using CodingExercisePDE.Domain.ServiceBus;
using CodingExercisePDE.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingExercisePDE.ServiceBusMessaging
{
    public class ServiceBusConsumerCreated : IServiceBusConsumer
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _clientLocal;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Entities.RandomNumber> _repository;
        private readonly QueueClient _queueClient;
        private const string QUEUE_NAME = "numbergenerated";
        private readonly ILogger _logger;

        public ServiceBusConsumerCreated(
            IConfiguration configuration,
            IRepository<Entities.RandomNumber> repository,
            IHttpClientFactory httpClientFactory,
            ILogger<ServiceBusConsumerCreated> logger)
        {
            _configuration = configuration;
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _clientLocal = _httpClientFactory.CreateClient("local");
            _logger = logger;
            _queueClient = new QueueClient(_configuration.GetConnectionString("ServiceBusConnectionString"), QUEUE_NAME);
        }

        public void RegisterOnMessageHandlerAndReceiveMessages(object param = null)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            MessagePayload myPayload = null;
            try
            {
                myPayload = JsonConvert.DeserializeObject<MessagePayload>(Encoding.UTF8.GetString(message.Body));

                var repo = new Entities.RandomNumber
                {
                    MessageId = myPayload.Id,                    
                    Number = myPayload.Number,
                    CreatedAt = myPayload.Created,
                };

                _logger.LogDebug($"Read from Queue: {myPayload}");
                                
                bool canSaveToDb = true;
                _repository.Add(repo);

                if (myPayload.Number > 800)
                {
                    CancellationToken cts = new CancellationTokenSource().Token;

                    //post to endpoint                    
                    var dto = new NumberDto
                    {
                        Id = myPayload.Id,
                        Number = myPayload.Number,
                        CreatedAt = myPayload.Created,
                    };
                    var json = JsonConvert.SerializeObject(dto);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _clientLocal.PostAsync("api/numbers", data, cts).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {                        
                        //only save if posted sussfully
                    }
                    else
                    {
                        canSaveToDb = false;
                        _logger.LogDebug($"StatusCode {response.StatusCode} during posting message {myPayload}");
                        //failed to process message. Dont remove from queue
                        return;
                    }
                }
                if (canSaveToDb)
                {
                    if (await _repository.SaveChangesAsync())
                        _logger.LogDebug($"{nameof(ProcessMessagesAsync)}: {repo} saved to DB");
                    else
                        _logger.LogWarning($"Failed to save to DB {nameof(ProcessMessagesAsync)}");
                }               

                await _queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process {myPayload}");
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _queueClient.CloseAsync();
        }
    }
}
