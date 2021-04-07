using System.Collections.Immutable;
using KidsTown.KidsTown.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace KidsTown.Application.Models
{
    public class CheckInOutRequest
    {
        public string SecurityCode { get; init; } = string.Empty;
        public long EventId { get; init; }
        public IImmutableList<int> SelectedLocationIds { get; init; } = ImmutableList<int>.Empty;
        public bool IsFastCheckInOut { get; init; } = true;
        public CheckType CheckType { get; init; }
        public IImmutableList<CheckInOutCandidate> CheckInOutCandidates { get; init; } = ImmutableList<CheckInOutCandidate>.Empty;
    }
}