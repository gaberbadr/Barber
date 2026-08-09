using API.Helpers;
using Application.Features.Barbers.Commands.UpdateBookingSettings;
using Application.Features.Barbers.Commands.UpdateWorkingHours;
using Application.Features.Barbers.Queries.GetAll;
using Application.Features.Barbers.Queries.GetById;
using Application.Features.Barbers.Queries.GetMyBookings;
using Application.Features.Bookings.Queries.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Barbers;
using Application.Common.Models;

namespace API.Controllers
{
    public class BarbersController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly CurrentUser _currentUser;

        public BarbersController(IMediator mediator, CurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Get all barbers (public).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllBarbersQuery());
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get a specific barber by ID (public).
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _mediator.Send(new GetBarberByIdQuery { BarberId = id });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get available time slots for a barber on a specific date (public).
        /// </summary>
        [HttpGet("{id}/availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailability(string id, [FromQuery] DateOnly date)
        {
            var result = await _mediator.Send(new GetAvailableSlotsQuery
            {
                BarberId = id,
                Date = date
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get the current barber's own profile.
        /// </summary>
        [Authorize(Roles = "Barber")]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _mediator.Send(new GetBarberByIdQuery { BarberId = _currentUser.UserId! });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Update the current barber's booking settings.
        /// </summary>
        [Authorize(Roles = "Barber")]
        [HttpPut("me/booking-settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateBookingSettings([FromBody] UpdateBookingSettingsRequest request)
        {
            var command = new UpdateBookingSettingsCommand
            {
                BarberId = _currentUser.UserId!,
                BookingDurationMinutes = request.BookingDurationMinutes,
                AcceptingBookings = request.AcceptingBookings
            };

            var result = await _mediator.Send(command);
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Update the current barber's working hours.
        /// </summary>
        [Authorize(Roles = "Barber")]
        [HttpPut("me/working-hours")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateWorkingHours([FromBody] UpdateWorkingHoursRequest request)
        {
            var command = new UpdateWorkingHoursCommand
            {
                BarberId = _currentUser.UserId!,
                WorkingHours = request.WorkingHours.Select(w => new WorkingHourInput
                {
                    DayOfWeek = w.DayOfWeek,
                    OpeningTime = w.OpeningTime,
                    ClosingTime = w.ClosingTime,
                    IsClosed = w.IsClosed
                }).ToList()
            };

            var result = await _mediator.Send(command);
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get the current barber's bookings.
        /// </summary>
        [Authorize(Roles = "Barber")]
        [HttpGet("me/bookings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings([FromQuery] GetMyBookingsApiRequest request)
        {
            var result = await _mediator.Send(new GetMyBarberBookingsQuery
            {
                BarberId = _currentUser.UserId!,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                PageIndex = request.PageNumber,
                PageSize = request.PageSize
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

    }
}