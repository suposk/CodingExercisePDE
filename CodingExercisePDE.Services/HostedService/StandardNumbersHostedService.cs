﻿using CodingExercisePDE.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pde.CodingExercise.RandomNumberGenerator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingExercisePDE.Services.HostedService
{
    public class StandardNumbersHostedService : BackgroundService
    {
        private readonly IRepository<RandomNumber> _repository;
        private readonly ILogger<StandardNumbersHostedService> _logger;
        private readonly NumberGenerator _numberGenerator = new NumberGenerator();        

        public StandardNumbersHostedService(
            IRepository<RandomNumber> repository,
            ILogger<StandardNumbersHostedService> logger)
        {
            _repository = repository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _numberGenerator.NumberGenerated += OnNumberGenerated;
        }

        private async void OnNumberGenerated(object sender, NumberGeneratedEventArgs e)
        {
            try
            {
                _logger.LogDebug($"{nameof(OnNumberGenerated)}: {e.Number}");

                var repo = new RandomNumber(e.Number);
                _repository.Add(repo);
                if (await _repository.SaveChangesAsync())
                {
                    _logger.LogDebug($"{nameof(OnNumberGenerated)}: {e.Number} saved to DB");                    
                }
                else
                {
                    _logger.LogWarning($"Failed to save to DB {nameof(OnNumberGenerated)}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(OnNumberGenerated)}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StandardNumbersHostedService is starting.");
            _numberGenerator.Start(); _logger.LogInformation("_numberGenerator is starting.");

            stoppingToken.Register(() => _logger.LogInformation("#1 Register StandardNumbersHostedService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                int sec = 30;
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