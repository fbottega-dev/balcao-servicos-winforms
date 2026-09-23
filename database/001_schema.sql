-- Execute no banco escolhido para o Balcão de Serviços.
-- O programa não cria nem apaga tabelas automaticamente.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nome nvarchar(80) NOT NULL,
        Telefone nvarchar(20) NOT NULL CONSTRAINT DF_Clientes_Telefone DEFAULT N'',
        CONSTRAINT CK_Clientes_Nome CHECK (LEN(LTRIM(RTRIM(Nome))) >= 2)
    );
END;

IF OBJECT_ID(N'dbo.OrdensServico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrdensServico
    (
        Id int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_OrdensServico PRIMARY KEY,
        ClienteId int NOT NULL,
        Equipamento nvarchar(80) NOT NULL,
        Descricao nvarchar(500) NOT NULL,
        Situacao int NOT NULL CONSTRAINT DF_OrdensServico_Situacao DEFAULT 0,
        ValorFinal decimal(8, 2) NULL,
        CriadaEm datetime2 NOT NULL CONSTRAINT DF_OrdensServico_CriadaEm DEFAULT SYSUTCDATETIME(),
        EncerradaEm datetime2 NULL,
        Versao rowversion NOT NULL,
        CONSTRAINT FK_OrdensServico_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes(Id),
        CONSTRAINT CK_OrdensServico_Equipamento CHECK (LEN(LTRIM(RTRIM(Equipamento))) >= 2),
        CONSTRAINT CK_OrdensServico_Descricao CHECK (LEN(LTRIM(RTRIM(Descricao))) >= 5),
        CONSTRAINT CK_OrdensServico_Situacao CHECK (Situacao IN (0, 1, 2, 3)),
        CONSTRAINT CK_OrdensServico_Encerramento CHECK
        (
            (Situacao IN (0, 1) AND EncerradaEm IS NULL AND ValorFinal IS NULL)
            OR (Situacao = 2 AND EncerradaEm IS NOT NULL AND ValorFinal IS NOT NULL AND ValorFinal >= 0)
            OR (Situacao = 3 AND EncerradaEm IS NOT NULL AND ValorFinal IS NULL)
        )
    );
    CREATE INDEX IX_OrdensServico_ClienteId ON dbo.OrdensServico (ClienteId);
    CREATE INDEX IX_OrdensServico_Situacao_CriadaEm ON dbo.OrdensServico (Situacao, CriadaEm DESC, Id DESC);
END;

IF OBJECT_ID(N'dbo.HistoricosOrdem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HistoricosOrdem
    (
        Id int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_HistoricosOrdem PRIMARY KEY,
        OrdemId int NOT NULL,
        Descricao nvarchar(400) NOT NULL,
        CriadoEm datetime2 NOT NULL CONSTRAINT DF_HistoricosOrdem_CriadoEm DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_HistoricosOrdem_OrdensServico FOREIGN KEY (OrdemId) REFERENCES dbo.OrdensServico(Id)
    );
    CREATE INDEX IX_HistoricosOrdem_OrdemId ON dbo.HistoricosOrdem (OrdemId, CriadoEm, Id);
END;

COMMIT TRANSACTION;
