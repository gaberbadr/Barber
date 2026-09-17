using Application.Common.Models;
using Application.Features.Coupons.Queries.GetByCode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [AllowAnonymous]
    public class CouponsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CouponsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get the value of a valid coupon by its code.
        /// </summary>
        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCouponValue(string code)
        {
            var result = await _mediator.Send(new GetCouponByCodeQuery { Code = code });
            if (result.IsError) return HandleErrorResult(result.Errors);

            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }
    }
}
