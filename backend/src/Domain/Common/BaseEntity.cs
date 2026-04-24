namespace Portal.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }

    public void MarcarComoAtualizado()
    {
        DataAtualizacao = DateTime.UtcNow;
    }
}
