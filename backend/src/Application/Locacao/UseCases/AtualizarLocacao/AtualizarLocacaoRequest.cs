using Portal.Application.Locacao.Common;
using Portal.Domain.Base;

namespace Portal.Application.Locacao.UseCases.AtualizarLocacao;

public class AtualizarLocacaoRequest : LocacaoRequest
{
    public Guid Id { get; set; }


    public override bool IsValid()
    {
        return Validate(this, new AtualizarLocacaoValidator());
    }
}
