using Application.Common.Models;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandleErrorResult(IReadOnlyList<ErrorOr.Error> errors)
        {
            var firstError = errors.FirstOrDefault();
            
            var apiResponse = ApiResponse<object>.FailureResponse(firstError.Description);

            return firstError.Type switch
            {
                ErrorOr.ErrorType.NotFound => NotFound(apiResponse),
                ErrorOr.ErrorType.Validation => BadRequest(apiResponse),
                ErrorOr.ErrorType.Conflict => Conflict(apiResponse),
                ErrorOr.ErrorType.Unauthorized => Unauthorized(apiResponse),
                ErrorOr.ErrorType.Forbidden => Forbid(),
                ErrorOr.ErrorType.Failure => BadRequest(apiResponse),
                _ => StatusCode(500, apiResponse)
            };
        }
    }
}
