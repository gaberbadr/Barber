using API.Helpers;
using Application.Features.Admin.Dashboard.Commands.ToggleUserBlock;
using Application.Features.Admin.Dashboard.Commands.UpdateSettings;
using Application.Features.Admin.Dashboard.Queries.GetAllBookings;
using Application.Features.Admin.Dashboard.Queries.GetAllUsers;
using Application.Features.Admin.Dashboard.Queries.GetDashboardStats;
using Application.Features.Admin.Dashboard.Queries.GetMonthlyReport;
using Application.Features.Admin.Dashboard.Queries.GetSettings;
using Application.Features.Admin.Dashboard.Queries.GetTopBarbers;
using Application.Features.Admin.Dashboard.Queries.GetTopServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get dashboard statistics.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetDashboardStatsQuery());
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get monthly report data.
        /// </summary>
        [HttpGet("dashboard/monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] string period = "ThisMonth",
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            var result = await _mediator.Send(new GetMonthlyReportQuery
            {
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get most booked barbers.
        /// </summary>
        [HttpGet("dashboard/top-barbers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopBarbers([FromQuery] int count = 10)
        {
            var result = await _mediator.Send(new GetTopBarbersQuery { Count = count });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get most requested services.
        /// </summary>
        [HttpGet("dashboard/top-services")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopServices([FromQuery] int count = 10)
        {
            var result = await _mediator.Send(new GetTopServicesQuery { Count = count });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Search/filter all bookings.
        /// </summary>
        [HttpGet("bookings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookings(
            [FromQuery] DateOnly? date,
            [FromQuery] string? barberId,
            [FromQuery] string? customerId,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAllBookingsQuery
            {
                Date = date,
                BarberId = barberId,
                CustomerId = customerId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get all users with search/filter.
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? searchTerm,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAllUsersQuery
            {
                SearchTerm = searchTerm,
                IsActive = isActive,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Block a user.
        /// </summary>
        [HttpPut("users/{id}/block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BlockUser(string id)
        {
            var result = await _mediator.Send(new ToggleUserBlockCommand
            {
                UserId = id,
                Block = true
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(new { message = "User blocked successfully." });
        }

        /// <summary>
        /// Unblock a user.
        /// </summary>
        [HttpPut("users/{id}/unblock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var result = await _mediator.Send(new ToggleUserBlockCommand
            {
                UserId = id,
                Block = false
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(new { message = "User unblocked successfully." });
        }

        /// <summary>
        /// Get global booking settings.
        /// </summary>
        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _mediator.Send(new GetGlobalSettingsQuery());
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        /// <summary>
        /// Update global booking settings.
        /// </summary>
        [HttpPut("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateGlobalSettingsRequest request)
        {
            var result = await _mediator.Send(new UpdateGlobalSettingsCommand
            {
                MaximumBookingAdvanceDays = request.MaximumBookingAdvanceDays,
                CancellationWindowHours = request.CancellationWindowHours
            });
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(result.Value);
        }

        private IActionResult HandleErrorResult(IReadOnlyList<ErrorOr.Error> errors)
        {
            var firstError = errors.FirstOrDefault();
            return firstError.Type switch
            {
                ErrorOr.ErrorType.NotFound => NotFound(new { message = firstError.Description }),
                ErrorOr.ErrorType.Validation => BadRequest(new { message = firstError.Description }),
                ErrorOr.ErrorType.Conflict => Conflict(new { message = firstError.Description }),
                ErrorOr.ErrorType.Unauthorized => Unauthorized(new { message = firstError.Description }),
                ErrorOr.ErrorType.Forbidden => Forbid(),
                _ => StatusCode(500, new { message = firstError.Description })
            };
        }
    }

    public class UpdateGlobalSettingsRequest
    {
        public int MaximumBookingAdvanceDays { get; set; }
        public int CancellationWindowHours { get; set; }
    }
}