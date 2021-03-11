using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IConfigurationService
    {
        Task<ImmutableList<Location>> GetActiveLocations();
        long GetDefaultEventId();
        Task<ImmutableList<CheckInsEvent>> GetAvailableEvents();
    }
}