-- ===================================
-- SCRIPT PARA INSERIR ESCOPOS L6A
-- Sistema de Locação de Equipamentos
-- ===================================

-- Inserir escopos do sistema L6A
INSERT INTO sso.escopo (nome, descricao) VALUES
-- Escopos de Categoria
('categoria.ler', 'Listar e visualizar categorias'),
('categoria.criar', 'Criar novas categorias'),
('categoria.atualizar', 'Atualizar categorias existentes'),
('categoria.inativar', 'Inativar categorias'),

-- Escopos de Equipamento
('equipamento.ler', 'Listar e visualizar equipamentos'),
('equipamento.criar', 'Criar novos equipamentos'),
('equipamento.atualizar', 'Atualizar equipamentos existentes'),
('equipamento.inativar', 'Inativar equipamentos'),

-- Escopos de Cliente
('cliente.ler', 'Listar e visualizar clientes'),
('cliente.criar', 'Criar novos clientes'),
('cliente.atualizar', 'Atualizar clientes existentes'),
('cliente.bloquear', 'Bloquear clientes'),
('cliente.desbloquear', 'Desbloquear clientes'),

-- Escopos de Locação
('locacao.ler', 'Listar e visualizar locações'),
('locacao.criar', 'Criar novas locações'),
('locacao.atualizar', 'Atualizar locações existentes'),
('locacao.devolver', 'Devolver equipamentos locados'),
('locacao.cancelar', 'Cancelar locações'),

-- Escopos de Financeiro
('financeiro.ler', 'Visualizar lançamentos financeiros')

ON CONFLICT (nome) DO NOTHING;

-- Criar perfil L6A Admin (acesso completo)
INSERT INTO sso.perfil (nome, descricao, ativo, ismater, tenant_id, parceiro_id) VALUES
('L6A Admin', 'Perfil com acesso total ao sistema L6A', true, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (nome) DO NOTHING;

-- Criar perfil L6A Operador (acesso limitado)
INSERT INTO sso.perfil (nome, descricao, ativo, ismater, tenant_id, parceiro_id) VALUES
('L6A Operador', 'Perfil com acesso operacional ao sistema L6A', true, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (nome) DO NOTHING;

-- Criar perfil L6A Consulta (somente leitura)
INSERT INTO sso.perfil (nome, descricao, ativo, ismater, tenant_id, parceiro_id) VALUES
('L6A Consulta', 'Perfil com acesso somente leitura ao sistema L6A', true, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (nome) DO NOTHING;

-- ===================================
-- ASSOCIAR ESCOPOS AOS PERFIS
-- ===================================

-- L6A Admin: Todos os escopos
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Admin'),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler', 'categoria.criar', 'categoria.atualizar', 'categoria.inativar',
    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar', 'equipamento.inativar',
    'cliente.ler', 'cliente.criar', 'cliente.atualizar', 'cliente.bloquear', 'cliente.desbloquear',
    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver', 'locacao.cancelar',
    'financeiro.ler'
)
ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

-- L6A Operador: Escopos operacionais (sem inativar/bloquear)
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Operador'),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler', 'categoria.criar', 'categoria.atualizar',
    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar',
    'cliente.ler', 'cliente.criar', 'cliente.atualizar',
    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver',
    'financeiro.ler'
)
ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

-- L6A Consulta: Apenas leitura
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Consulta'),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler',
    'equipamento.ler',
    'cliente.ler',
    'locacao.ler',
    'financeiro.ler'
)
ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

-- ===================================
-- VERIFICAÇÃO FINAL
-- ===================================

-- Verificar escopos inseridos
SELECT COUNT(*) as escopos_l6a_inseridos 
FROM sso.escopo 
WHERE nome LIKE 'categoria.%' OR nome LIKE 'equipamento.%' OR nome LIKE 'cliente.%' OR nome LIKE 'locacao.%' OR nome LIKE 'financeiro.%';

-- Verificar perfis criados
SELECT id, nome, descricao, ativo FROM sso.perfil WHERE nome LIKE 'L6A%';

-- Verificar associações perfil-escopo
SELECT 
    p.nome as perfil_nome,
    COUNT(pe.escopo_id) as total_escopos
FROM sso.perfil p
LEFT JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
WHERE p.nome LIKE 'L6A%'
GROUP BY p.id, p.nome
ORDER BY p.nome;

-- Exibir todos os escopos por perfil
SELECT 
    p.nome as perfil,
    e.nome as escopo,
    e.descricao as descricao_escopo
FROM sso.perfil p
INNER JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
INNER JOIN sso.escopo e ON pe.escopo_id = e.id
WHERE p.nome LIKE 'L6A%'
ORDER BY p.nome, e.nome;

-- Mensagem final
DO $$ 
BEGIN 
    RAISE NOTICE 'Escopos L6A criados e associados aos perfis com sucesso!';
    RAISE NOTICE 'Perfis disponíveis: L6A Admin, L6A Operador, L6A Consulta';
    RAISE NOTICE 'Total de escopos L6A: 16 escopos';
END $$;