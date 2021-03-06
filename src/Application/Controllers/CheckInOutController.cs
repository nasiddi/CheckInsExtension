﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.Common;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckInOutController : ControllerBase
    {
        private readonly ICheckInOutService _checkInOutService;
        private readonly ITaskManagementService _taskManagementService;

        public CheckInOutController(ICheckInOutService checkInOutService, ITaskManagementService taskManagementService)
        {
            _checkInOutService = checkInOutService;
            _taskManagementService = taskManagementService;
        }

        [HttpPost]
        [Route("manual")]
        [Produces("application/json")]
        public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutRequest request)
        {
            var attendanceIds = request.CheckInOutCandidates.Select(c => c.AttendanceId).ToImmutableList();
            var success = await _checkInOutService.CheckInOutPeople(checkType: request.CheckType, attendanceIds: attendanceIds);
            
            var names = request.CheckInOutCandidates.Select(c => c.Name).ToImmutableList();

            if (success)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"{request.CheckType.ToString()} für {string.Join(separator: ", ", values: names)} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                    AttendanceIds = attendanceIds
                });
            }

            return Ok(new CheckInOutResult
            {
                Text = $"{request.CheckType.ToString()} für {string.Join(separator: ", ", values: names)} ist fehlgeschlagen",
                AlertLevel = AlertLevel.Danger
            });
        }
        

        [HttpPost]
        [Route("people")]
        [Produces("application/json")]
        public async Task<IActionResult> GetPeople([FromBody] CheckInOutRequest request)
        {
            _taskManagementService.ActivateBackgroundTasks();
            
            var people = await _checkInOutService.SearchForPeople(
                new PeopleSearchParameters(
                    securityCode: request.SecurityCode, 
                    eventId: request.EventId,
                    locationGroups: request.SelectedLocationIds)).ConfigureAwait(false);

            if (people.Count == 0)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Es wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations und CheckIn/CheckOut Einstellungen überprüfen.",
                    AlertLevel = AlertLevel.Danger
                });
            }
            
            var peopleReadyForProcessing = GetPeopleInRequestedState(people: people, checkType: request.CheckType);
            
            if (peopleReadyForProcessing.Count == 0)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Für {request.CheckType.ToString()} wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations und CheckIn/CheckOut Einstellungen überprüfen.",
                    AlertLevel = AlertLevel.Danger
                });
            }
            
            if (request.IsFastCheckInOut 
                && (!peopleReadyForProcessing.Any(p => !p.MayLeaveAlone || p.HasPeopleWithoutPickupPermission) 
                    || request.CheckType == CheckType.CheckIn))
            {
                var kid = await TryFastCheckInOut(people: peopleReadyForProcessing, checkType: request.CheckType).ConfigureAwait(false);
                if (kid != null)
                {
                    return Ok(new CheckInOutResult
                    {
                        Text = $"{request.CheckType.ToString()} für {kid.FirstName} {kid.LastName} war erfolgreich.",
                        AlertLevel = AlertLevel.Success,
                        SuccessfulFastCheckout = true,
                        AttendanceIds = ImmutableList.Create(kid.AttendanceId)
                    });
                }
            }

            var checkInOutCandidates = peopleReadyForProcessing.Select(p => new CheckInOutCandidate
            {
                AttendanceId = p.AttendanceId,
                Name = $"{p.FirstName} {p.LastName}",
                MayLeaveAlone = p.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = p.HasPeopleWithoutPickupPermission
            }).ToImmutableList();

            var text = GetCandidateAlert(request: request, checkInOutCandidates: checkInOutCandidates, level: out var level);

            return Ok(new CheckInOutResult
            {
                Text = text,
                AlertLevel = level,
                CheckInOutCandidates = checkInOutCandidates
            });
        }
        
        [HttpPost]
        [Route("undo/{checkType}")]
        [Produces("application/json")]
        public async Task<IActionResult> Undo([FromRoute] CheckType checkType, [FromBody] IImmutableList<int> attendanceIds)
        {
            var checkState = checkType switch
            {
                CheckType.GuestCheckIn => CheckState.None,
                CheckType.CheckIn => CheckState.PreCheckedIn,
                CheckType.CheckOut => CheckState.CheckedIn,
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
            };
               
            var success = await _checkInOutService.UndoAction(revertedCheckState: checkState, attendanceIds: attendanceIds);

            if (success)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Der {checkType} wurde erfolgreich rückgänig gemacht.",
                    AlertLevel = AlertLevel.Info
                });    
            }
            
            return Ok(new CheckInOutResult
            {
                Text = "Rückgänig machen ist fehlgeschlagen",
                AlertLevel = AlertLevel.Danger
            }); 
            
        }

        [HttpPost]
        [Route("guest/checkin")]
        [Produces("application/json")]
        public async Task<IActionResult> CheckInGuest([FromBody] GuestCheckInRequest request)
        {
            var attendanceId = await _checkInOutService.CreateGuest(
                locationId: request.LocationId,
                securityCode: request.SecurityCode,
                firstName: request.FirstName,
                lastName: request.LastName);

            if (attendanceId == null)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                    AlertLevel = AlertLevel.Danger
                });
            }

            await _checkInOutService.CheckInOutPeople(
                checkType: CheckType.CheckIn, 
                attendanceIds: ImmutableList.Create(attendanceId.Value));

            return Ok(new CheckInOutResult
            {
                Text = $"CheckIn für {request.FirstName} {request.LastName} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = ImmutableList.Create(attendanceId.Value)
            });
        }
        
        [HttpPost]
        [Route("guest/create")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateGuest([FromBody] GuestCheckInRequest request)
        {
            var attendanceId = await _checkInOutService.CreateGuest(
                locationId: request.LocationId,
                securityCode: request.SecurityCode,
                firstName: request.FirstName,
                lastName: request.LastName);

            if (attendanceId == null)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                    AlertLevel = AlertLevel.Danger
                });
            }
            
            return Ok(new CheckInOutResult
            {
                Text = $"Erfassen von {request.FirstName} {request.LastName} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = ImmutableList.Create(attendanceId.Value)
            });

        }
        
        private static string GetCandidateAlert(CheckInOutRequest request, IImmutableList<CheckInOutCandidate> checkInOutCandidates,
            out AlertLevel level)
        {
            var text = "";
            level = AlertLevel.Info;

            if (request.CheckType != CheckType.CheckOut)
            {
                return text;
            }
            
            if (checkInOutCandidates.Any(c => !c.MayLeaveAlone))
            {
                text = "Kinder mit gelbem Hintergrund dürfen nicht alleine gehen";
                level = AlertLevel.Warning;
            }

            if (!checkInOutCandidates.Any(c => c.HasPeopleWithoutPickupPermission))
            {
                return text;
            }
            
            text = "Bei Kindern mit rotem Hintergrund gibt es Personen, die nicht abholberechtigt sind";
            level = AlertLevel.Danger;

            return text;
        }

        private async Task<Kid?> TryFastCheckInOut(IImmutableList<Kid> people, CheckType checkType)
        {
            if (people.Count != 1)
            {
                return null;
            }
            
            var success = await _checkInOutService
                .CheckInOutPeople(checkType: checkType, attendanceIds: people.Select(p => p.AttendanceId).ToImmutableList())
                .ConfigureAwait(false);
                
            return success ? people.Single() : null;

        }

        private static IImmutableList<Kid> GetPeopleInRequestedState(IImmutableList<Kid> people, CheckType checkType)
        {
            return checkType switch
            {
                CheckType.CheckIn => people.Where(p => p.CheckState == CheckState.PreCheckedIn).ToImmutableList(),
                CheckType.CheckOut => people.Where(p => p.CheckState == CheckState.CheckedIn).ToImmutableList(),
                CheckType.GuestCheckIn => throw new ArgumentException(message: $"Unexpected CheckType: {checkType}", paramName: nameof(checkType)),
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
            };
        }
    }
}