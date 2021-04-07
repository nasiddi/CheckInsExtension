using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.Application.Controllers;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.Database;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace KidsTown.IntegrationTests
{
    public class CheckInOutControllerTest
    {
        private ServiceProvider _serviceProvider = null!;
        
        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider);
        }
        
        [Test]
        public async Task GetPeople_()
        {
            SetupServiceProvider();
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider);
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider);

            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);

            var request = new CheckInOutRequest
            {
                SecurityCode = "H1R1",
                EventId = 389697,
                SelectedLocationIds = ImmutableList.Create(1,2,3,4,5),
                IsFastCheckInOut = false,
                CheckType = CheckType.CheckIn,
                CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty
            };

            await controller.GetPeople(request: request);
        }
        
        private void SetupServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: false)
                .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
                .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
                .Build();

            services.AddSingleton<IConfiguration>(implementationFactory: _ => configuration);
            
            services.AddScoped<ICheckInOutService, CheckInOutService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOverviewService, OverviewService>();
            
            services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IOverviewRepository, OverviewRepository>();

            services.AddDbContext<KidsTownContext>(
                contextLifetime: ServiceLifetime.Transient,
                optionsAction: o
                    => o.UseSqlServer(connectionString: configuration.GetConnectionString(name: "Database")));
            
            _serviceProvider =  services.BuildServiceProvider();
        }
    }
}