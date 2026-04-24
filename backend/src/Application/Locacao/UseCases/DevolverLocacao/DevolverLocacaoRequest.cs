using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Locacao.UseCases.DevolverLocacao;

public class DevolverLocacaoRequest : BaseRequest
{
    public Guid Id { get; set; }
    public DateTime DataDevolucao { get; set; }
    public string? Observacao { get; set; }

    public override bool IsValid()
    {
        var validator = new InlineValidator<DevolverLocacaoRequest>();
        validator.RuleFor(x => x.Id).NotEmpty();
        validator.RuleFor(x => x.DataDevolucao).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        validator.RuleFor(x => x.Observacao).MaximumLength(500);
        return Validate(this, validator);
    }
}
