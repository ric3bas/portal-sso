
using System.Reflection;

namespace Portal.Domain.Parceiro
{
    public class ParceiroQuery
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

    }
}
