-- ===================================
-- SCRIPT SQL PARA TESTE E VERIFICAÇÃO
-- Sistema de Locação de Equipamentos  
-- ===================================

-- Verificar se o esquema foi criado
SELECT name FROM sys.schemas WHERE name = 'l6a'

-- Verificar se todas as tabelas foram criadas
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'l6a'
ORDER BY TABLE_NAME

-- Testar inserção de dados de exemplo
BEGIN TRANSACTION

-- Inserir categoria de teste
DECLARE @CategoriaId UNIQUEIDENTIFIER = NEWID()
INSERT INTO l6a.Categoria (Id, Nome, Ativo) VALUES (@CategoriaId, 'Teste - Equipamentos de Som', 1)

-- Inserir equipamento de teste  
DECLARE @EquipamentoId UNIQUEIDENTIFIER = NEWID()
INSERT INTO l6a.Equipamento (Id, Nome, CategoriaId, QuantidadeEstoque, PrecoDiaria, Marca, Modelo, NumeroSerie, AnoFabricacao, Ativo)
VALUES (@EquipamentoId, 'Caixa de Som Teste', @CategoriaId, 5, 50.00, 'JBL', 'EON612', 'TEST001', 2023, 1)

-- Inserir cliente de teste
DECLARE @ClienteId UNIQUEIDENTIFIER = NEWID()
INSERT INTO l6a.Cliente (Id, Nome, Cpf, Email, Ativo)
VALUES (@ClienteId, 'Cliente Teste', '12345678901', 'teste@email.com', 1)

-- Inserir telefone do cliente
INSERT INTO l6a.Telefone (Id, ClienteId, Ddd, Numero)
VALUES (NEWID(), @ClienteId, '11', '999887766')

-- Inserir endereço do cliente
INSERT INTO l6a.Endereco (Id, ClienteId, Logradouro, Cidade, Estado, Numero)
VALUES (NEWID(), @ClienteId, 'Rua Teste', 'São Paulo', 'SP', '123')

-- Inserir locação de teste
DECLARE @LocacaoId UNIQUEIDENTIFIER = NEWID()
INSERT INTO l6a.Locacao (Id, ClienteId, EquipamentoId, Status, DataRetirada, PrevisaoDevolucao, ValorDiaria)
VALUES (@LocacaoId, @ClienteId, @EquipamentoId, 1, GETDATE(), DATEADD(day, 3, GETDATE()), 50.00)

-- Simular devolução e inserir no financeiro
UPDATE l6a.Locacao 
SET Status = 2, DataDevolucaoReal = DATEADD(day, 2, DataRetirada)
WHERE Id = @LocacaoId

-- Inserir lançamento financeiro
INSERT INTO l6a.Financeiro (Id, LocacaoId, ClienteId, EquipamentoId, DataRetirada, DataDevolucao, DiasLocados, ValorDiaria, ValorTotal)
SELECT NEWID(), l.Id, l.ClienteId, l.EquipamentoId, l.DataRetirada, l.DataDevolucaoReal,
       DATEDIFF(day, l.DataRetirada, l.DataDevolucaoReal) + 1,
       l.ValorDiaria,
       (DATEDIFF(day, l.DataRetirada, l.DataDevolucaoReal) + 1) * l.ValorDiaria
FROM l6a.Locacao l
WHERE l.Id = @LocacaoId

-- Verificar dados inseridos
SELECT 'Categorias' as Tabela, COUNT(*) as Total FROM l6a.Categoria
UNION ALL
SELECT 'Equipamentos', COUNT(*) FROM l6a.Equipamento
UNION ALL  
SELECT 'Clientes', COUNT(*) FROM l6a.Cliente
UNION ALL
SELECT 'Telefones', COUNT(*) FROM l6a.Telefone
UNION ALL
SELECT 'Enderecos', COUNT(*) FROM l6a.Endereco
UNION ALL
SELECT 'Locacoes', COUNT(*) FROM l6a.Locacao
UNION ALL
SELECT 'Financeiro', COUNT(*) FROM l6a.Financeiro

-- Testar view de relatório
SELECT TOP 5 * FROM l6a.vw_RelatorioLocacoes

-- Testar view de equipamentos disponíveis
SELECT TOP 5 * FROM l6a.vw_EquipamentosDisponiveis

-- Testar procedure de estatísticas
EXEC l6a.sp_ObterEstatisticas

ROLLBACK TRANSACTION -- Desfaz as inserções de teste

PRINT 'Teste concluído com sucesso!'
PRINT 'Execute COMMIT TRANSACTION em vez de ROLLBACK se quiser manter os dados de teste'