using CodingExercisePDE.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pde.CodingExercise.RandomNumberGenerator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using CodingExercisePDE.Domain;
using Newtonsoft.Json;
using CodingExercisePDE.Domain.ServiceBus;

namespace CodingExercisePDE.Services.HostedService
{
    public class StandardNumbersHostedService : BackgroundService
    {
        private readonly IServiceBusSender _serviceBusSender;
        private readonly ILogger<StandardNumbersHostedService> _logger;
        private NumberGenerator _numberGenerator;        

        public StandardNumbersHostedService(
            IServiceBusSender serviceBusSender,
            ILogger<StandardNumbersHostedService> logger)
        {
            _numberGenerator = new NumberGenerator();
            _numberGenerator.NumberGenerated += OnNumberGenerated;
            _serviceBusSender = serviceBusSender;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));              
        }

        private async void OnNumberGenerated(object sender, NumberGeneratedEventArgs e)
        {
            try
            {
                _logger.LogDebug($"{nameof(OnNumberGenerated)}: {e.Number}");

                var mes = new MessagePayload
                {
                    Id = Guid.NewGuid(),
                    ServiceBusMessageType = ServiceBusMessageType.Created,
                    Number = e.Number,                    
                    Created = DateTime.UtcNow,
                };
                await _serviceBusSender.SendMessage(mes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(OnNumberGenerated)}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StandardNumbersHostedService is starting.");


            await Task.Delay(2 * 1000);
            _numberGenerator.Start(); _logger.LogInformation("_numberGenerator is starting.");

            stoppingToken.Register(() => _logger.LogInformation("#1 Register StandardNumbersHostedService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                int sec = 60;
                _logger.LogDebug($"StandardNumbersHostedService background task is doing background work every {sec} sec.");
                                
                await Task.Delay(sec * 1000, stoppingToken);
            }

            _logger.LogInformation("StandardNumbersHostedService background task is stopping.");

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _numberGenerator.NumberGenerated -= OnNumberGenerated;
            _numberGenerator.Stop();
            return base.StopAsync(cancellationToken);
        }
    }
}
