using Portal.Domain.Base;
using Portal.Features.Categoria.Domain.Validations;

namespace Portal.Features.Categoria.Domain
{
    public class CategoriaRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new CategoriaRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }

    public class AtualizarCategoriaRequest : BaseRequest
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool? Ativo { get; set; }

        public override bool IsValid()
        {
            var validator = new AtualizarCategoriaRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }
}