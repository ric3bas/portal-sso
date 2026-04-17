-- ===================================
-- SCRIPT DE CORREÇÃO E ATUALIZAÇÃO COMPLETA
-- Esquema l6a - Sistema de Locação de Equipamentos
-- ===================================

-- Primeiro, criar o esquema se não existir
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'l6a')
BEGIN
    EXEC('CREATE SCHEMA l6a')
    PRINT 'Esquema l6a criado com sucesso!'
END
ELSE
BEGIN
    PRINT 'Esquema l6a já existe!'
END
GO

-- Verificar se as tabelas existem e criar se necessário
-- TABELA CATEGORIA
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Categoria')
BEGIN
    CREATE TABLE l6a.Categoria (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Nome NVARCHAR(100) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NULL
    )
    
    CREATE INDEX IX_Categoria_Nome ON l6a.Categoria (Nome)
    CREATE INDEX IX_Categoria_Ativo ON l6a.Categoria (Ativo)
    ALTER TABLE l6a.Categoria ADD CONSTRAINT UQ_Categoria_Nome UNIQUE (Nome)
    
    PRINT 'Tabela l6a.Categoria criada!'
END
GO

-- TABELA EQUIPAMENTO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Equipamento')
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
    
    CREATE INDEX IX_Equipamento_Nome ON l6a.Equipamento (Nome)
    CREATE INDEX IX_Equipamento_CategoriaId ON l6a.Equipamento (CategoriaId)
    CREATE INDEX IX_Equipamento_Marca ON l6a.Equipamento (Marca)
    CREATE INDEX IX_Equipamento_Modelo ON l6a.Equipamento (Modelo)
    CREATE INDEX IX_Equipamento_Ativo ON l6a.Equipamento (Ativo)
    ALTER TABLE l6a.Equipamento ADD CONSTRAINT UQ_Equipamento_NumeroSerie UNIQUE (NumeroSerie)
    
    PRINT 'Tabela l6a.Equipamento criada!'
END
GO

-- TABELA CLIENTE
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Cliente')
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
    
    CREATE INDEX IX_Cliente_Nome ON l6a.Cliente (Nome)
    CREATE INDEX IX_Cliente_Cpf ON l6a.Cliente (Cpf)
    CREATE INDEX IX_Cliente_Email ON l6a.Cliente (Email)
    CREATE INDEX IX_Cliente_Bloqueado ON l6a.Cliente (Bloqueado)
    CREATE INDEX IX_Cliente_Ativo ON l6a.Cliente (Ativo)
    ALTER TABLE l6a.Cliente ADD CONSTRAINT UQ_Cliente_Cpf UNIQUE (Cpf)
    ALTER TABLE l6a.Cliente ADD CONSTRAINT UQ_Cliente_Email UNIQUE (Email)
    
    PRINT 'Tabela l6a.Cliente criada!'
END
GO

-- TABELA TELEFONE
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Telefone')
BEGIN
    CREATE TABLE l6a.Telefone (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ClienteId UNIQUEIDENTIFIER NOT NULL,
        Ddd NVARCHAR(2) NOT NULL,
        Numero NVARCHAR(9) NOT NULL,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_Telefone_Cliente FOREIGN KEY (ClienteId) REFERENCES l6a.Cliente(Id) ON DELETE CASCADE
    )
    
    CREATE INDEX IX_Telefone_ClienteId ON l6a.Telefone (ClienteId)
    CREATE INDEX IX_Telefone_DddNumero ON l6a.Telefone (Ddd, Numero)
    
    PRINT 'Tabela l6a.Telefone criada!'
END
GO

-- TABELA ENDERECO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Endereco')
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
    
    CREATE INDEX IX_Endereco_ClienteId ON l6a.Endereco (ClienteId)
    CREATE INDEX IX_Endereco_Cidade ON l6a.Endereco (Cidade)
    CREATE INDEX IX_Endereco_Estado ON l6a.Endereco (Estado)
    
    PRINT 'Tabela l6a.Endereco criada!'
END
GO

-- TABELA LOCACAO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Locacao')
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
    
    CREATE INDEX IX_Locacao_ClienteId ON l6a.Locacao (ClienteId)
    CREATE INDEX IX_Locacao_EquipamentoId ON l6a.Locacao (EquipamentoId)
    CREATE INDEX IX_Locacao_Status ON l6a.Locacao (Status)
    CREATE INDEX IX_Locacao_DataRetirada ON l6a.Locacao (DataRetirada)
    CREATE INDEX IX_Locacao_PrevisaoDevolucao ON l6a.Locacao (PrevisaoDevolucao)
    CREATE INDEX IX_Locacao_DataDevolucaoReal ON l6a.Locacao (DataDevolucaoReal)
    CREATE INDEX IX_Locacao_EquipamentoId_Status_Datas ON l6a.Locacao (EquipamentoId, Status, DataRetirada, PrevisaoDevolucao)
    
    PRINT 'Tabela l6a.Locacao criada!'
END
GO

-- TABELA FINANCEIRO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'l6a' AND TABLE_NAME = 'Financeiro')
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
    
    CREATE INDEX IX_Financeiro_LocacaoId ON l6a.Financeiro (LocacaoId)
    CREATE INDEX IX_Financeiro_ClienteId ON l6a.Financeiro (ClienteId)
    CREATE INDEX IX_Financeiro_EquipamentoId ON l6a.Financeiro (EquipamentoId)
    CREATE INDEX IX_Financeiro_DataLancamento ON l6a.Financeiro (DataLancamento)
    CREATE INDEX IX_Financeiro_DataDevolucao ON l6a.Financeiro (DataDevolucao)
    ALTER TABLE l6a.Financeiro ADD CONSTRAINT UQ_Financeiro_LocacaoId UNIQUE (LocacaoId)
    
    PRINT 'Tabela l6a.Financeiro criada!'
END
GO

-- Inserir dados iniciais de categoria se não existirem
IF NOT EXISTS (SELECT 1 FROM l6a.Categoria)
BEGIN
    INSERT INTO l6a.Categoria (Id, Nome, Ativo) VALUES
    (NEWID(), 'Equipamentos de Som', 1),
    (NEWID(), 'Equipamentos de Iluminação', 1),
    (NEWID(), 'Equipamentos de Vídeo', 1),
    (NEWID(), 'Ferramentas', 1),
    (NEWID(), 'Equipamentos de Segurança', 1),
    (NEWID(), 'Equipamentos de Jardinagem', 1),
    (NEWID(), 'Equipamentos de Limpeza', 1),
    (NEWID(), 'Equipamentos Eletrônicos', 1)
    
    PRINT 'Dados iniciais de categoria inseridos!'
END
GO

-- Verificar estrutura criada
SELECT 
    'ESQUEMA CRIADO' as Status,
    COUNT(*) as TotalTabelas
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'l6a'

SELECT 
    TABLE_NAME as Tabela,
    TABLE_TYPE as Tipo
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'l6a'
ORDER BY TABLE_NAME

PRINT 'Estrutura do banco de dados l6a verificada e configurada!'
PRINT 'Sistema pronto para uso!'