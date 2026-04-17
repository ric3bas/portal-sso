-- ===================================
-- ASSOCIAR ESCOPOS L6A AOS PERFIS
-- ===================================

-- L6A Admin: Todos os escopos
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Admin' LIMIT 1),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler', 'categoria.criar', 'categoria.atualizar', 'categoria.inativar',
    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar', 'equipamento.inativar',
    'cliente.ler', 'cliente.criar', 'cliente.atualizar', 'cliente.bloquear', 'cliente.desbloquear',
    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver', 'locacao.cancelar',
    'financeiro.ler'
);

-- L6A Operador: Escopos operacionais (sem inativar/bloquear)
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Operador' LIMIT 1),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler', 'categoria.criar', 'categoria.atualizar',
    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar',
    'cliente.ler', 'cliente.criar', 'cliente.atualizar',
    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver',
    'financeiro.ler'
);

-- L6A Consulta: Apenas leitura
INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
SELECT 
    (SELECT id FROM sso.perfil WHERE nome = 'L6A Consulta' LIMIT 1),
    e.id
FROM sso.escopo e
WHERE e.nome IN (
    'categoria.ler',
    'equipamento.ler',
    'cliente.ler',
    'locacao.ler',
    'financeiro.ler'
);

-- ===================================
-- VERIFICAÇÃO FINAL
-- ===================================

-- Verificar perfis L6A criados
SELECT id, nome, is_master FROM sso.perfil WHERE nome LIKE 'L6A%';

-- Verificar associações perfil-escopo
SELECT 
    p.nome as perfil_nome,
    COUNT(pe.escopo_id) as total_escopos
FROM sso.perfil p
LEFT JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
WHERE p.nome LIKE 'L6A%'
GROUP BY p.id, p.nome
ORDER BY p.nome;

-- Exibir todos os escopos por perfil L6A
SELECT 
    p.nome as perfil,
    e.nome as escopo
FROM sso.perfil p
INNER JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
INNER JOIN sso.escopo e ON pe.escopo_id = e.id
WHERE p.nome LIKE 'L6A%'
ORDER BY p.nome, e.nome;