---- ===================================
---- SCRIPT PARA INSERIR ESCOPOS L6A
---- Sistema de Locação de Equipamentos
---- ===================================

---- Inserir escopos do sistema L6A
--INSERT INTO sso.escopo (nome, is_master) VALUES
---- Escopos de Categoria
--('categoria.ler', false),
--('categoria.criar', false),
--('categoria.atualizar', false),
--('categoria.inativar', false),

---- Escopos de Equipamento
--('equipamento.ler', false),
--('equipamento.criar', false),
--('equipamento.atualizar', false),
--('equipamento.inativar', false),

---- Escopos de Cliente
--('cliente.ler', false),
--('cliente.criar', false),
--('cliente.atualizar', false),
--('cliente.bloquear', false),
--('cliente.desbloquear', false),

---- Escopos de Locação
--('locacao.ler', false),
--('locacao.criar', false),
--('locacao.atualizar', false),
--('locacao.devolver', false),
--('locacao.cancelar', false),

---- Escopos de Financeiro
--('financeiro.ler', false)

--ON CONFLICT (nome) DO NOTHING;

---- Criar perfis L6A
--INSERT INTO sso.perfil (nome, is_master) VALUES
--('L6A Admin', false),
--('L6A Operador', false),
--('L6A Consulta', false)
--ON CONFLICT (nome) DO NOTHING;

---- ===================================
---- ASSOCIAR ESCOPOS AOS PERFIS
---- ===================================

---- L6A Admin: Todos os escopos
--INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
--SELECT 
--    (SELECT id FROM sso.perfil WHERE nome = 'L6A Admin'),
--    e.id
--FROM sso.escopo e
--WHERE e.nome IN (
--    'categoria.ler', 'categoria.criar', 'categoria.atualizar', 'categoria.inativar',
--    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar', 'equipamento.inativar',
--    'cliente.ler', 'cliente.criar', 'cliente.atualizar', 'cliente.bloquear', 'cliente.desbloquear',
--    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver', 'locacao.cancelar',
--    'financeiro.ler'
--)
--ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

---- L6A Operador: Escopos operacionais (sem inativar/bloquear)
--INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
--SELECT 
--    (SELECT id FROM sso.perfil WHERE nome = 'L6A Operador'),
--    e.id
--FROM sso.escopo e
--WHERE e.nome IN (
--    'categoria.ler', 'categoria.criar', 'categoria.atualizar',
--    'equipamento.ler', 'equipamento.criar', 'equipamento.atualizar',
--    'cliente.ler', 'cliente.criar', 'cliente.atualizar',
--    'locacao.ler', 'locacao.criar', 'locacao.atualizar', 'locacao.devolver',
--    'financeiro.ler'
--)
--ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

---- L6A Consulta: Apenas leitura
--INSERT INTO sso.perfil_escopo (perfil_id, escopo_id)
--SELECT 
--    (SELECT id FROM sso.perfil WHERE nome = 'L6A Consulta'),
--    e.id
--FROM sso.escopo e
--WHERE e.nome IN (
--    'categoria.ler',
--    'equipamento.ler',
--    'cliente.ler',
--    'locacao.ler',
--    'financeiro.ler'
--)
--ON CONFLICT (perfil_id, escopo_id) DO NOTHING;

---- ===================================
---- VERIFICAÇÃO FINAL
---- ===================================

---- Verificar escopos inseridos
--SELECT COUNT(*) as escopos_l6a_inseridos 
--FROM sso.escopo 
--WHERE nome LIKE 'categoria.%' OR nome LIKE 'equipamento.%' OR nome LIKE 'cliente.%' OR nome LIKE 'locacao.%' OR nome LIKE 'financeiro.%';

---- Verificar perfis criados
--SELECT id, nome, is_master FROM sso.perfil WHERE nome LIKE 'L6A%';

---- Verificar associações perfil-escopo
--SELECT 
--    p.nome as perfil_nome,
--    COUNT(pe.escopo_id) as total_escopos
--FROM sso.perfil p
--LEFT JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
--WHERE p.nome LIKE 'L6A%'
--GROUP BY p.id, p.nome
--ORDER BY p.nome;

---- Exibir todos os escopos por perfil
--SELECT 
--    p.nome as perfil,
--    e.nome as escopo
--FROM sso.perfil p
--INNER JOIN sso.perfil_escopo pe ON p.id = pe.perfil_id
--INNER JOIN sso.escopo e ON pe.escopo_id = e.id
--WHERE p.nome LIKE 'L6A%'
--ORDER BY p.nome, e.nome;

---- Mensagem final
--DO $$ 
--BEGIN 
--    RAISE NOTICE 'Escopos L6A criados e associados aos perfis com sucesso!';
--    RAISE NOTICE 'Perfis disponíveis: L6A Admin, L6A Operador, L6A Consulta';
--    RAISE NOTICE 'Total de escopos L6A: 16 escopos';
--END $$;