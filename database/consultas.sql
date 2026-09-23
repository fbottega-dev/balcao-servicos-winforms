-- Execute no banco BalcaoServicos, depois de cadastrar algumas ordens no aplicativo.
-- Os exemplos apenas consultam os dados. Altere os valores das variáveis para praticar.
-- Situação: 0 = aberta, 1 = em andamento, 2 = concluída, 3 = cancelada.
-- As datas ficam armazenadas em UTC.

-- 1. Clientes em ordem alfabética: SELECT e ORDER BY.
SELECT Id, Nome, Telefone
FROM dbo.Clientes
ORDER BY Nome, Id;

-- 2. Ordens de uma situação, incluindo o nome do cliente: JOIN e WHERE.
DECLARE @Situacao int = 0;
DECLARE @Busca nvarchar(100) = N'';

SELECT TOP (100)
    ordem.Id,
    cliente.Nome AS Cliente,
    ordem.Equipamento,
    ordem.Descricao,
    ordem.CriadaEm
FROM dbo.OrdensServico AS ordem
INNER JOIN dbo.Clientes AS cliente ON cliente.Id = ordem.ClienteId
WHERE ordem.Situacao = @Situacao
  AND (@Busca = N''
       OR CHARINDEX(@Busca, cliente.Nome) > 0
       OR CHARINDEX(@Busca, ordem.Equipamento) > 0
       OR CHARINDEX(@Busca, ordem.Descricao) > 0)
ORDER BY ordem.CriadaEm DESC, ordem.Id DESC;

-- 3. Quantidade por situação: GROUP BY e COUNT.
SELECT
    CASE Situacao
        WHEN 0 THEN N'Aberta'
        WHEN 1 THEN N'Em andamento'
        WHEN 2 THEN N'Concluída'
        WHEN 3 THEN N'Cancelada'
    END AS Situacao,
    COUNT(*) AS Quantidade
FROM dbo.OrdensServico
GROUP BY Situacao
ORDER BY Situacao;

-- 4. Clientes com pelo menos dois serviços concluídos: GROUP BY, SUM e HAVING.
SELECT
    cliente.Id,
    cliente.Nome,
    COUNT(*) AS ServicosConcluidos,
    SUM(ordem.ValorFinal) AS ValorTotal
FROM dbo.Clientes AS cliente
INNER JOIN dbo.OrdensServico AS ordem ON ordem.ClienteId = cliente.Id
WHERE ordem.Situacao = 2
GROUP BY cliente.Id, cliente.Nome
HAVING COUNT(*) >= 2
ORDER BY ValorTotal DESC, cliente.Nome;

-- 5. Histórico de uma ordem: use um ID que apareça na lista do aplicativo.
DECLARE @OrdemId int = 1;

SELECT CriadoEm, Descricao
FROM dbo.HistoricosOrdem
WHERE OrdemId = @OrdemId
ORDER BY CriadoEm, Id;
