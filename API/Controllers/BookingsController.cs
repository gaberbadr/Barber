using API.Helpers;
using Application.Features.Bookings.Commands.Cancel;
using Application.Features.Bookings.Commands.Create;
using Application.Features.Bookings.Queries.GetById;
using Application.Features.Bookings.Queries.GetMyHistory;
using Application.Features.Bookings.Queries.GetMyUpcoming;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Bookings;
using Application.Common.Models;
using Application.Features.Bookings.Commands.UpdateStatus;

namespace API.Controllers
{
    [Authorize]
    public class BookingsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly CurrentUser _currentUser;

        public BookingsController(IMediator mediator, CurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Create a new booking.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var command = new CreateBookingCommand
            {
                CustomerId = _currentUser.UserId!,
                BarberId = request.BarberId,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                ServiceIds = request.ServiceIds,
                CouponCode = request.CouponCode,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _mediator.Send(command);
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get current user's upcoming bookings.
        /// </summary>
        [HttpGet("my/upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyUpcoming()
        {
            var result = await _mediator.Send(new GetMyUpcomingBookingsQuery
            {
                CustomerId = _currentUser.UserId!
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get current user's booking history (paginated).
        /// </summary>
        [HttpGet("my/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyHistory([FromQuery] GetMyHistoryApiRequest request)
        {
            var result = await _mediator.Send(new GetMyBookingHistoryQuery
            {
                CustomerId = _currentUser.UserId!,
                PageIndex = request.PageNumber,
                PageSize = request.PageSize
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get booking by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetBookingByIdQuery
            {
                BookingId = id,
                RequestingUserId = _currentUser.UserId!
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Cancel a booking.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _mediator.Send(new CancelBookingCommand
            {
                BookingId = id,
                CancelledByUserId = _currentUser.UserId!
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }
        /// <summary>
        /// Update booking status.
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, Barber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            var command = new UpdateBookingStatusCommand
            {
                BookingId = id,
                NewStatus = request.Status,
                RequestingUserId = _currentUser.UserId!
            };
            var result = await _mediator.Send(command);
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }
    }
}