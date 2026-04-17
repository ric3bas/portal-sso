-- ===================================
-- CONFIGURAÇÃO DE CONNECTION STRING
-- Para o Sistema de Locação de Equipamentos - Esquema l6a
-- ===================================

/*
INSTRUÇÕES PARA CONFIGURAÇÃO:

1. No appsettings.json, adicione ou atualize a connection string:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortalSSO_Locacao;Trusted_Connection=true;MultipleActiveResultSets=true",
    "SSO_POSTGRES": "Server=(localdb)\\mssqllocaldb;Database=PortalSSO_Locacao;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}

Para SQL Server:
"Server=localhost;Database=PortalSSO_Locacao;Trusted_Connection=true;MultipleActiveResultSets=true"

Para SQL Server com usuário/senha:
"Server=localhost;Database=PortalSSO_Locacao;User Id=sa;Password=SuaSenha;MultipleActiveResultSets=true;TrustServerCertificate=true"

Para Azure SQL:
"Server=seuservidor.database.windows.net;Database=PortalSSO_Locacao;User Id=seuusuario;Password=suasenha;MultipleActiveResultSets=true;TrustServerCertificate=true"

2. Execute o script setup_complete_l6a_schema.sql no banco de dados

3. O sistema está configurado para usar o esquema l6a em todas as operações
*/

-- Script de verificação da estrutura criada
USE PortalSSO_Locacao
GO

-- Verificar se o esquema l6a existe
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'l6a')
    PRINT '✓ Esquema l6a existe'
ELSE
    PRINT '✗ Esquema l6a NÃO existe - Execute o script setup_complete_l6a_schema.sql'
GO

-- Verificar tabelas criadas
SELECT 
    'Tabela' = t.TABLE_NAME,
    'Status' = CASE WHEN t.TABLE_NAME IS NOT NULL THEN '✓ Existe' ELSE '✗ Não existe' END,
    'Registros' = ISNULL(p.rows, 0)
FROM (
    SELECT 'Categoria' as TABLE_NAME
    UNION SELECT 'Equipamento'
    UNION SELECT 'Cliente' 
    UNION SELECT 'Telefone'
    UNION SELECT 'Endereco'
    UNION SELECT 'Locacao'
    UNION SELECT 'Financeiro'
) expected
LEFT JOIN INFORMATION_SCHEMA.TABLES t ON t.TABLE_NAME = expected.TABLE_NAME AND t.TABLE_SCHEMA = 'l6a'
LEFT JOIN sys.tables st ON st.name = expected.TABLE_NAME AND st.schema_id = SCHEMA_ID('l6a')
LEFT JOIN sys.dm_db_partition_stats p ON p.object_id = st.object_id AND p.index_id < 2
ORDER BY expected.TABLE_NAME

-- Verificar constraints e índices
SELECT 
    'Constraints' as Tipo,
    COUNT(*) as Total
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_SCHEMA = 'l6a'
UNION ALL
SELECT 
    'Índices',
    COUNT(*)
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID('l6a')
AND i.type > 0

PRINT 'Verificação completa!'
PRINT 'Se todas as tabelas existem, o sistema está pronto para usar!'