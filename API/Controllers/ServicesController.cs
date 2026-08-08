using Application.Features.Services.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
            if (result.IsError)
            {
                var firstError = result.FirstError;
                return StatusCode(500, new { message = firstError.Description });
            }
            return Ok(result.Value);
        }
    }
}