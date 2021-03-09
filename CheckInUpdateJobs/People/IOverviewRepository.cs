using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IOverviewRepository
    {
        Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocations,
            long eventId, DateTime date);

        Task<ImmutableList<Attendee>> GetAttendanceHistory(IImmutableList<int> selectedLocations,
            long eventId, DateTime startDate, DateTime endDate);
    }
}