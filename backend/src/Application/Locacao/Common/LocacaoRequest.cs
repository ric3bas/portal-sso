using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Locacao.Common
{
    public class LocacaoRequest : BaseRequest
    {
        public string ClienteId { get; set; } = string.Empty;
        public string EquipamentoId { get; set; } = string.Empty;
        public DateTime DataRetirada { get; set; }
        public DateTime PrevisaoDevolucao { get; set; }
        public decimal ValorDiaria { get; set; }
        public string? Observacao { get; set; }

        public override bool IsValid()
        {
            var validator = new InlineValidator<LocacaoRequest>();
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
}
