-- ===================================
-- CRIAÇÃO DO ESQUEMA E ESTRUTURA COMPLETA 
-- Sistema de Locação de Equipamentos
-- ===================================

-- Criar o esquema l6a
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'l6a')
BEGIN
    EXEC('CREATE SCHEMA l6a')
END
GO

-- ===================================
-- TABELA CATEGORIA
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categoria' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Categoria (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Nome NVARCHAR(100) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL
    )

    -- Índices
    CREATE INDEX IX_Categoria_Nome ON l6a.Categoria (Nome)
    CREATE INDEX IX_Categoria_Ativo ON l6a.Categoria (Ativo)
    
    -- Constraint único para nome
    ALTER TABLE l6a.Categoria ADD CONSTRAINT UQ_Categoria_Nome UNIQUE (Nome)
END
GO

-- ===================================
-- TABELA EQUIPAMENTO  
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Equipamento' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Equipamento (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Nome NVARCHAR(200) NOT NULL,
        CategoriaId UNIQUEIDENTIFIER NOT NULL,
        QuantidadeEstoque INT NOT NULL DEFAULT 0,
        PrecoDiaria DECIMAL(18,2) NOT NULL,
        Marca NVARCHAR(100) NOT NULL,
        Modelo NVARCHAR(100) NOT NULL,
        NumeroSerie NVARCHAR(50) NOT NULL,
        AnoFabricacao INT NOT NULL,
        Descricao NVARCHAR(500) NULL,
        ObservacaoInternas NVARCHAR(1000) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL,
        
        CONSTRAINT FK_Equipamento_Categoria FOREIGN KEY (CategoriaId) REFERENCES l6a.Categoria(Id)
    )

    -- Índices
    CREATE INDEX IX_Equipamento_Nome ON l6a.Equipamento (Nome)
    CREATE INDEX IX_Equipamento_CategoriaId ON l6a.Equipamento (CategoriaId)
    CREATE INDEX IX_Equipamento_Marca ON l6a.Equipamento (Marca)
    CREATE INDEX IX_Equipamento_Modelo ON l6a.Equipamento (Modelo)
    CREATE INDEX IX_Equipamento_Ativo ON l6a.Equipamento (Ativo)
    
    -- Constraint único para número de série
    ALTER TABLE l6a.Equipamento ADD CONSTRAINT UQ_Equipamento_NumeroSerie UNIQUE (NumeroSerie)
END
GO

-- ===================================
-- TABELA CLIENTE
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cliente' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Cliente (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Nome NVARCHAR(200) NOT NULL,
        Cpf NVARCHAR(11) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        Observacao NVARCHAR(500) NULL,
        Bloqueado BIT NOT NULL DEFAULT 0,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL
    )

    -- Índices
    CREATE INDEX IX_Cliente_Nome ON l6a.Cliente (Nome)
    CREATE INDEX IX_Cliente_Cpf ON l6a.Cliente (Cpf)
    CREATE INDEX IX_Cliente_Email ON l6a.Cliente (Email)
    CREATE INDEX IX_Cliente_Bloqueado ON l6a.Cliente (Bloqueado)
    CREATE INDEX IX_Cliente_Ativo ON l6a.Cliente (Ativo)
    
    -- Constraints únicos
    ALTER TABLE l6a.Cliente ADD CONSTRAINT UQ_Cliente_Cpf UNIQUE (Cpf)
    ALTER TABLE l6a.Cliente ADD CONSTRAINT UQ_Cliente_Email UNIQUE (Email)
END
GO

-- ===================================
-- TABELA TELEFONE
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Telefone' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Telefone (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ClienteId UNIQUEIDENTIFIER NOT NULL,
        Ddd NVARCHAR(2) NOT NULL,
        Numero NVARCHAR(9) NOT NULL,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_Telefone_Cliente FOREIGN KEY (ClienteId) REFERENCES l6a.Cliente(Id) ON DELETE CASCADE
    )

    -- Índices
    CREATE INDEX IX_Telefone_ClienteId ON l6a.Telefone (ClienteId)
    CREATE INDEX IX_Telefone_DddNumero ON l6a.Telefone (Ddd, Numero)
END
GO

-- ===================================
-- TABELA ENDERECO
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Endereco' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Endereco (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ClienteId UNIQUEIDENTIFIER NOT NULL,
        Logradouro NVARCHAR(200) NOT NULL,
        Cidade NVARCHAR(100) NOT NULL,
        Estado NVARCHAR(2) NOT NULL,
        Numero NVARCHAR(10) NOT NULL,
        Complemento NVARCHAR(100) NULL,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_Endereco_Cliente FOREIGN KEY (ClienteId) REFERENCES l6a.Cliente(Id) ON DELETE CASCADE
    )

    -- Índices
    CREATE INDEX IX_Endereco_ClienteId ON l6a.Endereco (ClienteId)
    CREATE INDEX IX_Endereco_Cidade ON l6a.Endereco (Cidade)
    CREATE INDEX IX_Endereco_Estado ON l6a.Endereco (Estado)
END
GO

-- ===================================
-- TABELA LOCACAO
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locacao' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Locacao (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ClienteId UNIQUEIDENTIFIER NOT NULL,
        EquipamentoId UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL, -- 1=Ativa, 2=Devolvida, 3=Atrasada, 4=Cancelada
        DataRetirada DATETIME2 NOT NULL,
        PrevisaoDevolucao DATETIME2 NOT NULL,
        DataDevolucaoReal DATETIME2 NULL,
        ValorDiaria DECIMAL(18,2) NOT NULL,
        Observacao NVARCHAR(500) NULL,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL,
        
        CONSTRAINT FK_Locacao_Cliente FOREIGN KEY (ClienteId) REFERENCES l6a.Cliente(Id),
        CONSTRAINT FK_Locacao_Equipamento FOREIGN KEY (EquipamentoId) REFERENCES l6a.Equipamento(Id),
        CONSTRAINT CK_Locacao_Status CHECK (Status IN (1,2,3,4)),
        CONSTRAINT CK_Locacao_DataRetirada_PrevisaoDevolucao CHECK (DataRetirada <= PrevisaoDevolucao)
    )

    -- Índices
    CREATE INDEX IX_Locacao_ClienteId ON l6a.Locacao (ClienteId)
    CREATE INDEX IX_Locacao_EquipamentoId ON l6a.Locacao (EquipamentoId)
    CREATE INDEX IX_Locacao_Status ON l6a.Locacao (Status)
    CREATE INDEX IX_Locacao_DataRetirada ON l6a.Locacao (DataRetirada)
    CREATE INDEX IX_Locacao_PrevisaoDevolucao ON l6a.Locacao (PrevisaoDevolucao)
    CREATE INDEX IX_Locacao_DataDevolucaoReal ON l6a.Locacao (DataDevolucaoReal)
    
    -- Índice composto para verificar disponibilidade do equipamento
    CREATE INDEX IX_Locacao_EquipamentoId_Status_Datas ON l6a.Locacao (EquipamentoId, Status, DataRetirada, PrevisaoDevolucao)
END
GO

-- ===================================
-- TABELA FINANCEIRO
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Financeiro' AND xtype='U')
BEGIN
    CREATE TABLE l6a.Financeiro (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        LocacaoId UNIQUEIDENTIFIER NOT NULL,
        ClienteId UNIQUEIDENTIFIER NOT NULL,
        EquipamentoId UNIQUEIDENTIFIER NOT NULL,
        DataRetirada DATETIME2 NOT NULL,
        DataDevolucao DATETIME2 NOT NULL,
        DiasLocados INT NOT NULL,
        ValorDiaria DECIMAL(18,2) NOT NULL,
        ValorTotal DECIMAL(18,2) NOT NULL,
        DataLancamento DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_Financeiro_Locacao FOREIGN KEY (LocacaoId) REFERENCES l6a.Locacao(Id),
        CONSTRAINT FK_Financeiro_Cliente FOREIGN KEY (ClienteId) REFERENCES l6a.Cliente(Id),
        CONSTRAINT FK_Financeiro_Equipamento FOREIGN KEY (EquipamentoId) REFERENCES l6a.Equipamento(Id),
        CONSTRAINT CK_Financeiro_DiasLocados CHECK (DiasLocados > 0),
        CONSTRAINT CK_Financeiro_ValorDiaria CHECK (ValorDiaria > 0),
        CONSTRAINT CK_Financeiro_ValorTotal CHECK (ValorTotal > 0)
    )

    -- Índices
    CREATE INDEX IX_Financeiro_LocacaoId ON l6a.Financeiro (LocacaoId)
    CREATE INDEX IX_Financeiro_ClienteId ON l6a.Financeiro (ClienteId)
    CREATE INDEX IX_Financeiro_EquipamentoId ON l6a.Financeiro (EquipamentoId)
    CREATE INDEX IX_Financeiro_DataLancamento ON l6a.Financeiro (DataLancamento)
    CREATE INDEX IX_Financeiro_DataDevolucao ON l6a.Financeiro (DataDevolucao)
    
    -- Constraint único para evitar lançamentos duplicados
    ALTER TABLE l6a.Financeiro ADD CONSTRAINT UQ_Financeiro_LocacaoId UNIQUE (LocacaoId)
END
GO

-- ===================================
-- DADOS INICIAIS - CATEGORIAS
-- ===================================
INSERT INTO l6a.Categoria (Id, Nome, Ativo) VALUES
(NEWID(), 'Equipamentos de Som', 1),
(NEWID(), 'Equipamentos de Iluminação', 1),
(NEWID(), 'Equipamentos de Vídeo', 1),
(NEWID(), 'Ferramentas', 1),
(NEWID(), 'Equipamentos de Segurança', 1),
(NEWID(), 'Equipamentos de Jardinagem', 1),
(NEWID(), 'Equipamentos de Limpeza', 1),
(NEWID(), 'Equipamentos Eletrônicos', 1)
GO

-- ===================================
-- PROCEDURES ÚTEIS
-- ===================================

-- Procedure para atualizar status de locações atrasadas
CREATE OR ALTER PROCEDURE l6a.sp_AtualizarLocacoesAtrasadas
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE l6a.Locacao 
    SET Status = 3, -- Atrasada
        DataAtualizacao = GETDATE()
    WHERE Status = 1 -- Ativa
    AND PrevisaoDevolucao < CAST(GETDATE() AS DATE)
END
GO

-- Procedure para obter estatísticas do sistema
CREATE OR ALTER PROCEDURE l6a.sp_ObterEstatisticas
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Categorias' as Entidade,
        COUNT(*) as Total,
        SUM(CASE WHEN Ativo = 1 THEN 1 ELSE 0 END) as Ativos
    FROM l6a.Categoria
    
    UNION ALL
    
    SELECT 
        'Equipamentos' as Entidade,
        COUNT(*) as Total,
        SUM(CASE WHEN Ativo = 1 THEN 1 ELSE 0 END) as Ativos
    FROM l6a.Equipamento
    
    UNION ALL
    
    SELECT 
        'Clientes' as Entidade,
        COUNT(*) as Total,
        SUM(CASE WHEN Ativo = 1 THEN 1 ELSE 0 END) as Ativos
    FROM l6a.Cliente
    
    UNION ALL
    
    SELECT 
        'Locações Ativas' as Entidade,
        COUNT(*) as Total,
        0 as Ativos
    FROM l6a.Locacao
    WHERE Status = 1
    
    UNION ALL
    
    SELECT 
        'Locações Atrasadas' as Entidade,
        COUNT(*) as Total,
        0 as Ativos
    FROM l6a.Locacao
    WHERE Status = 3
END
GO

-- ===================================
-- VIEWS ÚTEIS
-- ===================================

-- View para relatório de locações
CREATE OR ALTER VIEW l6a.vw_RelatorioLocacoes
AS
SELECT 
    l.Id as LocacaoId,
    c.Nome as ClienteNome,
    c.Cpf as ClienteCpf,
    e.Nome as EquipamentoNome,
    e.Marca as EquipamentoMarca,
    e.Modelo as EquipamentoModelo,
    cat.Nome as CategoriaNome,
    CASE l.Status 
        WHEN 1 THEN 'Ativa'
        WHEN 2 THEN 'Devolvida' 
        WHEN 3 THEN 'Atrasada'
        WHEN 4 THEN 'Cancelada'
    END as StatusDescricao,
    l.DataRetirada,
    l.PrevisaoDevolucao,
    l.DataDevolucaoReal,
    l.ValorDiaria,
    CASE 
        WHEN l.DataDevolucaoReal IS NOT NULL 
        THEN DATEDIFF(day, l.DataRetirada, l.DataDevolucaoReal) + 1
        ELSE NULL 
    END as DiasLocados,
    CASE 
        WHEN l.DataDevolucaoReal IS NOT NULL 
        THEN (DATEDIFF(day, l.DataRetirada, l.DataDevolucaoReal) + 1) * l.ValorDiaria
        ELSE NULL 
    END as ValorTotal,
    l.Observacao
FROM l6a.Locacao l
INNER JOIN l6a.Cliente c ON l.ClienteId = c.Id
INNER JOIN l6a.Equipamento e ON l.EquipamentoId = e.Id
INNER JOIN l6a.Categoria cat ON e.CategoriaId = cat.Id
GO

-- View para equipamentos disponíveis
CREATE OR ALTER VIEW l6a.vw_EquipamentosDisponiveis
AS
SELECT 
    e.Id,
    e.Nome,
    e.Marca,
    e.Modelo,
    e.PrecoDiaria,
    cat.Nome as CategoriaNome,
    e.QuantidadeEstoque,
    ISNULL(loc_ativas.QtdLocada, 0) as QuantidadeLocada,
    e.QuantidadeEstoque - ISNULL(loc_ativas.QtdLocada, 0) as QuantidadeDisponivel
FROM l6a.Equipamento e
INNER JOIN l6a.Categoria cat ON e.CategoriaId = cat.Id
LEFT JOIN (
    SELECT 
        EquipamentoId,
        COUNT(*) as QtdLocada
    FROM l6a.Locacao 
    WHERE Status IN (1, 3) -- Ativas e Atrasadas
    GROUP BY EquipamentoId
) loc_ativas ON e.Id = loc_ativas.EquipamentoId
WHERE e.Ativo = 1
GO

PRINT 'Estrutura do banco de dados criada com sucesso no esquema l6a!'
PRINT 'Tabelas criadas: Categoria, Equipamento, Cliente, Telefone, Endereco, Locacao, Financeiro'
PRINT 'Procedures criadas: sp_AtualizarLocacoesAtrasadas, sp_ObterEstatisticas'
PRINT 'Views criadas: vw_RelatorioLocacoes, vw_EquipamentosDisponiveis'