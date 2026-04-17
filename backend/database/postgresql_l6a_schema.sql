-- ===================================
-- SCRIPT PARA POSTGRESQL
-- Esquema l6a - Sistema de Locação de Equipamentos
-- ===================================

-- Habilitar extensão para UUID se não estiver habilitada
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Criar o esquema se não existir
CREATE SCHEMA IF NOT EXISTS l6a;

-- Definir o search_path para usar o esquema l6a
SET search_path TO l6a, public;

-- ===================================
-- TABELA CATEGORIA
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.categoria (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    nome VARCHAR(100) NOT NULL UNIQUE,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NULL
);

-- Índices para categoria
CREATE INDEX IF NOT EXISTS ix_categoria_nome ON l6a.categoria (nome);
CREATE INDEX IF NOT EXISTS ix_categoria_ativo ON l6a.categoria (ativo);

-- ===================================
-- TABELA EQUIPAMENTO
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.equipamento (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    nome VARCHAR(200) NOT NULL,
    categoria_id UUID NOT NULL,
    quantidade_estoque INTEGER NOT NULL DEFAULT 0,
    preco_diaria DECIMAL(18,2) NOT NULL,
    marca VARCHAR(100) NOT NULL,
    modelo VARCHAR(100) NOT NULL,
    numero_serie VARCHAR(50) NOT NULL UNIQUE,
    ano_fabricacao INTEGER NOT NULL,
    descricao TEXT NULL,
    observacao_internas TEXT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NULL,
    
    CONSTRAINT fk_equipamento_categoria FOREIGN KEY (categoria_id) REFERENCES l6a.categoria(id),
    CONSTRAINT ck_equipamento_ano_fabricacao CHECK (ano_fabricacao > 1900 AND ano_fabricacao <= EXTRACT(YEAR FROM NOW()) + 1),
    CONSTRAINT ck_equipamento_preco_diaria CHECK (preco_diaria > 0),
    CONSTRAINT ck_equipamento_quantidade_estoque CHECK (quantidade_estoque >= 0)
);

-- Índices para equipamento
CREATE INDEX IF NOT EXISTS ix_equipamento_nome ON l6a.equipamento (nome);
CREATE INDEX IF NOT EXISTS ix_equipamento_categoria_id ON l6a.equipamento (categoria_id);
CREATE INDEX IF NOT EXISTS ix_equipamento_marca ON l6a.equipamento (marca);
CREATE INDEX IF NOT EXISTS ix_equipamento_modelo ON l6a.equipamento (modelo);
CREATE INDEX IF NOT EXISTS ix_equipamento_ativo ON l6a.equipamento (ativo);

-- ===================================
-- TABELA CLIENTE
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.cliente (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    nome VARCHAR(200) NOT NULL,
    cpf VARCHAR(11) NOT NULL UNIQUE,
    email VARCHAR(200) NOT NULL UNIQUE,
    observacao TEXT NULL,
    bloqueado BOOLEAN NOT NULL DEFAULT false,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NULL,
    
    CONSTRAINT ck_cliente_cpf_length CHECK (length(cpf) = 11),
    CONSTRAINT ck_cliente_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- Índices para cliente
CREATE INDEX IF NOT EXISTS ix_cliente_nome ON l6a.cliente (nome);
CREATE INDEX IF NOT EXISTS ix_cliente_cpf ON l6a.cliente (cpf);
CREATE INDEX IF NOT EXISTS ix_cliente_email ON l6a.cliente (email);
CREATE INDEX IF NOT EXISTS ix_cliente_bloqueado ON l6a.cliente (bloqueado);
CREATE INDEX IF NOT EXISTS ix_cliente_ativo ON l6a.cliente (ativo);

-- ===================================
-- TABELA TELEFONE
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.telefone (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cliente_id UUID NOT NULL,
    ddd VARCHAR(2) NOT NULL,
    numero VARCHAR(9) NOT NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_telefone_cliente FOREIGN KEY (cliente_id) REFERENCES l6a.cliente(id) ON DELETE CASCADE,
    CONSTRAINT ck_telefone_ddd CHECK (length(ddd) = 2 AND ddd ~ '^[0-9]+$'),
    CONSTRAINT ck_telefone_numero CHECK (length(numero) BETWEEN 8 AND 9 AND numero ~ '^[0-9]+$')
);

-- Índices para telefone
CREATE INDEX IF NOT EXISTS ix_telefone_cliente_id ON l6a.telefone (cliente_id);
CREATE INDEX IF NOT EXISTS ix_telefone_ddd_numero ON l6a.telefone (ddd, numero);

-- ===================================
-- TABELA ENDERECO
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.endereco (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cliente_id UUID NOT NULL,
    logradouro VARCHAR(200) NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    estado VARCHAR(2) NOT NULL,
    numero VARCHAR(10) NOT NULL,
    complemento VARCHAR(100) NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_endereco_cliente FOREIGN KEY (cliente_id) REFERENCES l6a.cliente(id) ON DELETE CASCADE,
    CONSTRAINT ck_endereco_estado CHECK (length(estado) = 2)
);

-- Índices para endereco
CREATE INDEX IF NOT EXISTS ix_endereco_cliente_id ON l6a.endereco (cliente_id);
CREATE INDEX IF NOT EXISTS ix_endereco_cidade ON l6a.endereco (cidade);
CREATE INDEX IF NOT EXISTS ix_endereco_estado ON l6a.endereco (estado);

-- ===================================
-- TABELA LOCACAO
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.locacao (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cliente_id UUID NOT NULL,
    equipamento_id UUID NOT NULL,
    status INTEGER NOT NULL, -- 1=Ativa, 2=Devolvida, 3=Atrasada, 4=Cancelada
    data_retirada TIMESTAMP NOT NULL,
    previsao_devolucao TIMESTAMP NOT NULL,
    data_devolucao_real TIMESTAMP NULL,
    valor_diaria DECIMAL(18,2) NOT NULL,
    observacao TEXT NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NULL,
    
    CONSTRAINT fk_locacao_cliente FOREIGN KEY (cliente_id) REFERENCES l6a.cliente(id),
    CONSTRAINT fk_locacao_equipamento FOREIGN KEY (equipamento_id) REFERENCES l6a.equipamento(id),
    CONSTRAINT ck_locacao_status CHECK (status IN (1,2,3,4)),
    CONSTRAINT ck_locacao_data_retirada_previsao_devolucao CHECK (data_retirada <= previsao_devolucao),
    CONSTRAINT ck_locacao_valor_diaria CHECK (valor_diaria > 0)
);

-- Índices para locacao
CREATE INDEX IF NOT EXISTS ix_locacao_cliente_id ON l6a.locacao (cliente_id);
CREATE INDEX IF NOT EXISTS ix_locacao_equipamento_id ON l6a.locacao (equipamento_id);
CREATE INDEX IF NOT EXISTS ix_locacao_status ON l6a.locacao (status);
CREATE INDEX IF NOT EXISTS ix_locacao_data_retirada ON l6a.locacao (data_retirada);
CREATE INDEX IF NOT EXISTS ix_locacao_previsao_devolucao ON l6a.locacao (previsao_devolucao);
CREATE INDEX IF NOT EXISTS ix_locacao_data_devolucao_real ON l6a.locacao (data_devolucao_real);
CREATE INDEX IF NOT EXISTS ix_locacao_equipamento_status_datas ON l6a.locacao (equipamento_id, status, data_retirada, previsao_devolucao);

-- ===================================
-- TABELA FINANCEIRO
-- ===================================
CREATE TABLE IF NOT EXISTS l6a.financeiro (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    locacao_id UUID NOT NULL UNIQUE,
    cliente_id UUID NOT NULL,
    equipamento_id UUID NOT NULL,
    data_retirada TIMESTAMP NOT NULL,
    data_devolucao TIMESTAMP NOT NULL,
    dias_locados INTEGER NOT NULL,
    valor_diaria DECIMAL(18,2) NOT NULL,
    valor_total DECIMAL(18,2) NOT NULL,
    data_lancamento TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_financeiro_locacao FOREIGN KEY (locacao_id) REFERENCES l6a.locacao(id),
    CONSTRAINT fk_financeiro_cliente FOREIGN KEY (cliente_id) REFERENCES l6a.cliente(id),
    CONSTRAINT fk_financeiro_equipamento FOREIGN KEY (equipamento_id) REFERENCES l6a.equipamento(id),
    CONSTRAINT ck_financeiro_dias_locados CHECK (dias_locados > 0),
    CONSTRAINT ck_financeiro_valor_diaria CHECK (valor_diaria > 0),
    CONSTRAINT ck_financeiro_valor_total CHECK (valor_total > 0)
);

-- Índices para financeiro
CREATE INDEX IF NOT EXISTS ix_financeiro_locacao_id ON l6a.financeiro (locacao_id);
CREATE INDEX IF NOT EXISTS ix_financeiro_cliente_id ON l6a.financeiro (cliente_id);
CREATE INDEX IF NOT EXISTS ix_financeiro_equipamento_id ON l6a.financeiro (equipamento_id);
CREATE INDEX IF NOT EXISTS ix_financeiro_data_lancamento ON l6a.financeiro (data_lancamento);
CREATE INDEX IF NOT EXISTS ix_financeiro_data_devolucao ON l6a.financeiro (data_devolucao);

-- ===================================
-- INSERIR DADOS INICIAIS DE CATEGORIA
-- ===================================
INSERT INTO l6a.categoria (nome, ativo) VALUES
('Equipamentos de Som', true),
('Equipamentos de Iluminação', true),
('Equipamentos de Vídeo', true),
('Ferramentas', true),
('Equipamentos de Segurança', true),
('Equipamentos de Jardinagem', true),
('Equipamentos de Limpeza', true),
('Equipamentos Eletrônicos', true)
ON CONFLICT (nome) DO NOTHING;

-- ===================================
-- FUNÇÕES E PROCEDURES ÚTEIS
-- ===================================

-- Função para atualizar status de locações atrasadas
CREATE OR REPLACE FUNCTION l6a.atualizar_locacoes_atrasadas()
RETURNS INTEGER AS $$
DECLARE
    linhas_afetadas INTEGER;
BEGIN
    UPDATE l6a.locacao 
    SET status = 3, -- Atrasada
        data_atualizacao = NOW()
    WHERE status = 1 -- Ativa
    AND previsao_devolucao < CURRENT_DATE;
    
    GET DIAGNOSTICS linhas_afetadas = ROW_COUNT;
    RETURN linhas_afetadas;
END;
$$ LANGUAGE plpgsql;

-- Função para obter estatísticas do sistema
CREATE OR REPLACE FUNCTION l6a.obter_estatisticas()
RETURNS TABLE(entidade TEXT, total BIGINT, ativos BIGINT) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        'Categorias'::TEXT as entidade,
        COUNT(*) as total,
        SUM(CASE WHEN ativo = true THEN 1 ELSE 0 END) as ativos
    FROM l6a.categoria
    
    UNION ALL
    
    SELECT 
        'Equipamentos'::TEXT as entidade,
        COUNT(*) as total,
        SUM(CASE WHEN ativo = true THEN 1 ELSE 0 END) as ativos
    FROM l6a.equipamento
    
    UNION ALL
    
    SELECT 
        'Clientes'::TEXT as entidade,
        COUNT(*) as total,
        SUM(CASE WHEN ativo = true THEN 1 ELSE 0 END) as ativos
    FROM l6a.cliente
    
    UNION ALL
    
    SELECT 
        'Locações Ativas'::TEXT as entidade,
        COUNT(*) as total,
        0::BIGINT as ativos
    FROM l6a.locacao
    WHERE status = 1
    
    UNION ALL
    
    SELECT 
        'Locações Atrasadas'::TEXT as entidade,
        COUNT(*) as total,
        0::BIGINT as ativos
    FROM l6a.locacao
    WHERE status = 3;
END;
$$ LANGUAGE plpgsql;

-- ===================================
-- VIEWS ÚTEIS
-- ===================================

-- View para relatório de locações
CREATE OR REPLACE VIEW l6a.vw_relatorio_locacoes AS
SELECT 
    l.id as locacao_id,
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

-- View para equipamentos disponíveis
CREATE OR REPLACE VIEW l6a.vw_equipamentos_disponiveis AS
SELECT 
    e.id,
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

-- ===================================
-- VERIFICAÇÃO FINAL
-- ===================================

-- Verificar estrutura criada
SELECT 
    'ESQUEMA CRIADO' as status,
    COUNT(*) as total_tabelas
FROM information_schema.tables 
WHERE table_schema = 'l6a';

SELECT 
    table_name as tabela,
    table_type as tipo
FROM information_schema.tables 
WHERE table_schema = 'l6a'
ORDER BY table_name;

-- Verificar dados iniciais
SELECT COUNT(*) as categorias_inseridas FROM l6a.categoria;

-- Executar função de estatísticas
SELECT * FROM l6a.obter_estatisticas();

-- Mensagem final
DO $$ 
BEGIN 
    RAISE NOTICE 'Estrutura do banco de dados l6a criada com sucesso para PostgreSQL!';
    RAISE NOTICE 'Sistema pronto para uso!';
END $$;