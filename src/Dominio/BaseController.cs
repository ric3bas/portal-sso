using Microsoft.AspNetCore.Mvc;

namespace Portal.Dominio
{
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
	public abstract class BaseController : ControllerBase
	{
		protected ObjectResult BadRequestProblem(string detail, string title = "Erro de validação")
			=> Problem(
				statusCode: StatusCodes.Status400BadRequest,
				title: title,
				detail: detail,
				instance: HttpContext?.Request.Path);

		protected ObjectResult NotFoundProblem(string detail, string title = "Nenhum registro encontrado")
			=> Problem(
				statusCode: StatusCodes.Status404NotFound,
				title: title,
				detail: detail,
				instance: HttpContext?.Request.Path);

		protected ObjectResult InternalServerErrorProblem(string detail, string title = "Erro interno do servidor")
			=> Problem(
				statusCode: StatusCodes.Status500InternalServerError,
				title: title,
				detail: detail,
				instance: HttpContext?.Request.Path);

        protected ObjectResult BusinessErrorProblem(string detail, string title = "Erro de negócio")
			=> Problem(
				statusCode: StatusCodes.Status422UnprocessableEntity,
				title: title,
				detail: detail,
				instance: HttpContext?.Request.Path);
    }

	public sealed class ProducesBadRequestProblemAttribute : ProducesResponseTypeAttribute
	{
		public ProducesBadRequestProblemAttribute()
			: base(typeof(ProblemDetails), StatusCodes.Status400BadRequest)
		{
		}
	}

	public sealed class ProducesNotFoundProblemAttribute : ProducesResponseTypeAttribute
	{
		public ProducesNotFoundProblemAttribute()
			: base(typeof(ProblemDetails), StatusCodes.Status404NotFound)
		{
		}
	}

    public sealed class ProducesBusinessProblemAttribute : ProducesResponseTypeAttribute
    {
        public ProducesBusinessProblemAttribute()
            : base(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)
        {
        }
    }
}
