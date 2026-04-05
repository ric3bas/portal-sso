using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain.Exceptions;

namespace Portal.Domain.Base
{
	[ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status500InternalServerError)]
	public abstract class BaseController : ControllerBase
	{

		protected IActionResult HandleResult<T>(Result<T> result)
		{
			if (result.Success)
				return Ok(result.Data);

			if (result.Problem != null)
				result.Problem.TraceId = HttpContext.TraceIdentifier;

			return result.Type switch
			{
				ResultType.Validation => BadRequest(result.Problem),
				ResultType.Business => UnprocessableEntity(result.Problem),
				ResultType.NotFound => NotFound(result.Problem),
				_ => StatusCode(500, result.Problem)
			};
		}
    }

    public sealed class ProducesBadRequestProblemAttribute : ProducesResponseTypeAttribute
    {
        public ProducesBadRequestProblemAttribute()
            : base(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)
        {
        }
    }

    public sealed class ProducesNotFoundProblemAttribute : ProducesResponseTypeAttribute
	{
		public ProducesNotFoundProblemAttribute()
			: base(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)
		{
		}
	}

    public sealed class ProducesBusinessProblemAttribute : ProducesResponseTypeAttribute
    {
        public ProducesBusinessProblemAttribute()
            : base(typeof(CustomProblemDetails), StatusCodes.Status422UnprocessableEntity)
        {
        }
    }
}
