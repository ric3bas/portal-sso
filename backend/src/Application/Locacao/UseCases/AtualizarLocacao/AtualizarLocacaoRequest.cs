using FluentValidation;
using Portal.Application.Locacao.Common;
using Portal.Domain.Base;

namespace Portal.Application.Locacao.UseCases.AtualizarLocacao;

public class AtualizarLocacaoRequest : LocacaoRequest
{
    public Guid Id { get; set; }


    public override bool IsValid()
    {
        var validator = new InlineValidator<AtualizarLocacaoRequest>();
        validator.RuleFor(x => x.Id).NotEmpty();
        validator.RuleFor(x => x.ClienteId).NotNull().NotEmpty().Must(BeValidGuid);
        validator.RuleFor(x => x.EquipamentoId).NotNull().NotEmpty().Must(BeValidGuid);
        validator.RuleFor(x => x.DataRetirada).NotEmpty().GreaterThanOrEqualTo(DateTime.Today);
        validator.RuleFor(x => x.PrevisaoDevolucao).NotEmpty().GreaterThan(x => x.DataRetirada);
        validator.RuleFor(x => x.ValorDiaria).GreaterThan(0);
        validator.RuleFor(x => x.Observacao).MaximumLength(500);
        return Validate(this, validator);
    }

    private static bool BeValidGuid(string? guid) => Guid.TryParse(guid, out _);
}
