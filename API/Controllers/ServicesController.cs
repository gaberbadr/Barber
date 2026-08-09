using Application.Features.Services.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;

namespace API.Controllers
{
    public class ServicesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all active services (public).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllServicesQuery());
            if (result.IsError) return HandleErrorResult(result.Errors);
            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }
    }
}