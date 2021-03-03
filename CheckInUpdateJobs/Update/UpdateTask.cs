using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Threading.Tasks.Task;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateTask : IHostedService
    {
        private readonly ILogger<UpdateTask> _logger;
        private readonly IUpdateService _updateService;

        public bool TaskIsActive { get; set; } = false;

        public UpdateTask(ILogger<UpdateTask> logger, IUpdateService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime activationTime = new DateTime();
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!TaskIsActive)
                {
                    while (!TaskIsActive)
                    {
                        await Delay(5000, cancellationToken);
                    }
                    activationTime = DateTime.UtcNow;
                }

                await _updateService.FetchDataFromPlanningCenter();

                await Delay(5000, cancellationToken);

                if (DateTime.UtcNow - activationTime > TimeSpan.FromHours(12))
                {
                    TaskIsActive = false;
                }
            }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            return CompletedTask;        
        }
    }
}