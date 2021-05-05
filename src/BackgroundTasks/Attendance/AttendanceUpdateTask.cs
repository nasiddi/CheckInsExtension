using System.Threading.Tasks;
using KidsTown.BackgroundTasks.PlanningCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Attendance
{
    public class AttendanceUpdateTask : BackgroundTask
    {
        protected override string TaskName { get; } = nameof(AttendanceUpdateTask);
        protected override int Interval { get; } = 5000;

        private readonly IAttendanceUpdateService _attendanceUpdateService;

        public AttendanceUpdateTask(
            IAttendanceUpdateService attendanceUpdateService,
            IUpdateRepository updateRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(updateRepository: updateRepository, loggerFactory: loggerFactory, configuration: configuration)
        {
            _attendanceUpdateService = attendanceUpdateService;
        }

        protected override async Task<int> ExecuteRun()
        {
            return await _attendanceUpdateService.UpdateAttendance(daysLookBack: DaysLookBack).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}