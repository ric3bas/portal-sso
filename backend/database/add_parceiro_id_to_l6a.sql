-- ===================================
-- SCRIPT PARA ADICIONAR PARCEIRO_ID NAS TABELAS L6A
-- ===================================

-- Adicionar campo parceiro_id nas tabelas
ALTER TABLE l6a.categoria ADD COLUMN parceiro_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE l6a.equipamento ADD COLUMN parceiro_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE l6a.cliente ADD COLUMN parceiro_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE l6a.locacao ADD COLUMN parceiro_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE l6a.financeiro ADD COLUMN parceiro_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

-- Adicionar foreign keys para sso.parceiro
ALTER TABLE l6a.categoria ADD CONSTRAINT fk_categoria_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.equipamento ADD CONSTRAINT fk_equipamento_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.cliente ADD CONSTRAINT fk_cliente_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.locacao ADD CONSTRAINT fk_locacao_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.financeiro ADD CONSTRAINT fk_financeiro_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);

-- Adicionar índices para melhor performance
CREATE INDEX IF NOT EXISTS ix_categoria_parceiro_id ON l6a.categoria (parceiro_id);
CREATE INDEX IF NOT EXISTS ix_equipamento_parceiro_id ON l6a.equipamento (parceiro_id);
CREATE INDEX IF NOT EXISTS ix_cliente_parceiro_id ON l6a.cliente (parceiro_id);
CREATE INDEX IF NOT EXISTS ix_locacao_parceiro_id ON l6a.locacao (parceiro_id);
CREATE INDEX IF NOT EXISTS ix_financeiro_parceiro_id ON l6a.financeiro (parceiro_id);

-- Atualizar views para incluir parceiro_id
DROP VIEW IF EXISTS l6a.vw_relatorio_locacoes;
DROP VIEW IF EXISTS l6a.vw_equipamentos_disponiveis;

-- View para relatório de locações (atualizada)
CREATE OR REPLACE VIEW l6a.vw_relatorio_locacoes AS
SELECT 
    l.id as locacao_id,
    l.parceiro_id,
    c.nome as cliente_nome,
    c.cpf as cliente_cpf,
    e.nome as equipamento_nome,
    e.marca as equipamento_marca,
    e.modelo as equipamento_modelo,
    cat.nome as categoria_nome,
    CASE l.status 
        WHEN 1 THEN 'Ativa'
        WHEN 2 THEN 'Devolvida' 
        WHEN 3 THEN 'Atrasada'
        WHEN 4 THEN 'Cancelada'
    END as status_descricao,
    l.data_retirada,
    l.previsao_devolucao,
    l.data_devolucao_real,
    l.valor_diaria,
    CASE 
        WHEN l.data_devolucao_real IS NOT NULL 
        THEN EXTRACT(day FROM l.data_devolucao_real - l.data_retirada) + 1
        ELSE NULL 
    END as dias_locados,
    CASE 
        WHEN l.data_devolucao_real IS NOT NULL 
        THEN (EXTRACT(day FROM l.data_devolucao_real - l.data_retirada) + 1) * l.valor_diaria
        ELSE NULL 
    END as valor_total,
    l.observacao
FROM l6a.locacao l
INNER JOIN l6a.cliente c ON l.cliente_id = c.id
INNER JOIN l6a.equipamento e ON l.equipamento_id = e.id
INNER JOIN l6a.categoria cat ON e.categoria_id = cat.id;

-- View para equipamentos disponíveis (atualizada)
CREATE OR REPLACE VIEW l6a.vw_equipamentos_disponiveis AS
SELECT 
    e.id,
    e.parceiro_id,
    e.nome,
    e.marca,
    e.modelo,
    e.preco_diaria,
    cat.nome as categoria_nome,
    e.quantidade_estoque,
    COALESCE(loc_ativas.qtd_locada, 0) as quantidade_locada,
    e.quantidade_estoque - COALESCE(loc_ativas.qtd_locada, 0) as quantidade_disponivel
FROM l6a.equipamento e
INNER JOIN l6a.categoria cat ON e.categoria_id = cat.id
LEFT JOIN (
    SELECT 
        equipamento_id,
        COUNT(*) as qtd_locada
    FROM l6a.locacao 
    WHERE status IN (1, 3) -- Ativas e Atrasadas
    GROUP BY equipamento_id
) loc_ativas ON e.id = loc_ativas.equipamento_id
WHERE e.ativo = true;

NOTICE 'Campo parceiro_id adicionado em todas as tabelas l6a com sucesso!';