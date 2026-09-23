using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Balcao.Dados;
using Balcao.Dominio;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Servicos;
using NUnit.Framework;

namespace Balcao.Integracao
{
    [TestFixture]
    [NonParallelizable]
    public class RepositorioSqlServerTests
    {
        private string connectionString;
        private RepositorioSqlServer repositorio;
        private ServicoAtendimento servico;
        private Cliente cliente;
        private string identificador;

        [OneTimeSetUp]
        public void ExigirBancoDeTestesConfigurado()
        {
            connectionString = Environment.GetEnvironmentVariable("BALCAO_TEST_CONNECTION_STRING");
            Assert.That(connectionString, Is.Not.Null.And.Not.Empty,
                "Configure BALCAO_TEST_CONNECTION_STRING com um banco de testes que já tenha o schema.sql aplicado.");
        }

        [SetUp]
        public void PrepararDadosPropriosDoTeste()
        {
            cliente = null;
            identificador = Guid.NewGuid().ToString("N");
            repositorio = new RepositorioSqlServer(connectionString);
            servico = new ServicoAtendimento(repositorio);
            cliente = servico.CadastrarCliente("João D'Ávila " + identificador, "(41) 99999-0000");
        }

        [TearDown]
        public void RemoverSomenteOsRegistrosCriadosNesteTeste()
        {
            if (cliente == null || cliente.Id == 0)
                return;

            RemoverCliente(cliente.Id);
        }

        private void RemoverCliente(int clienteId)
        {
            using (var conexao = new SqlConnection(connectionString))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                using (var comando = conexao.CreateCommand())
                {
                    comando.Transaction = transacao;
                    comando.CommandText = @"
DELETE FROM dbo.HistoricosOrdem
WHERE OrdemId IN (SELECT Id FROM dbo.OrdensServico WHERE ClienteId = @clienteId);
DELETE FROM dbo.OrdensServico WHERE ClienteId = @clienteId;
DELETE FROM dbo.Clientes WHERE Id = @clienteId;";
                    comando.Parameters.Add("@clienteId", SqlDbType.Int).Value = clienteId;
                    comando.ExecuteNonQuery();
                    transacao.Commit();
                }
            }
        }

        [Test]
        public void ClienteSemTelefonePodeSerCadastradoERecarregado()
        {
            Cliente clienteSemTelefone = null;
            try
            {
                clienteSemTelefone = servico.CadastrarCliente("Cliente sem telefone " + identificador, null);

                var outraConexao = new RepositorioSqlServer(connectionString);
                var salvo = outraConexao.ListarClientes().Single(x => x.Id == clienteSemTelefone.Id);

                Assert.That(salvo.Nome, Is.EqualTo(clienteSemTelefone.Nome));
                Assert.That(salvo.Telefone, Is.EqualTo(string.Empty));
            }
            finally
            {
                if (clienteSemTelefone != null && clienteSemTelefone.Id > 0)
                    RemoverCliente(clienteSemTelefone.Id);
            }
        }

        [Test]
        public void OutroRepositorioRecarregaClienteOrdemEHistoricoPersistidos()
        {
            var ordem = servico.AbrirOrdem(cliente.Id, "Notebook", "Bateria não carrega");
            servico.IniciarOrdem(ordem.Id);
            servico.ConcluirOrdem(ordem.Id, 189.90m, "Conector substituído e carga testada");

            var outraConexao = new RepositorioSqlServer(connectionString);
            var clienteSalvo = outraConexao.ListarClientes().Single(x => x.Id == cliente.Id);
            var ordemSalva = outraConexao.ObterOrdem(ordem.Id);
            var historico = outraConexao.ListarHistorico(ordem.Id);

            Assert.That(clienteSalvo.Nome, Is.EqualTo(cliente.Nome));
            Assert.That(clienteSalvo.Telefone, Is.EqualTo("(41) 99999-0000"));
            Assert.That(ordemSalva.ClienteId, Is.EqualTo(cliente.Id));
            Assert.That(ordemSalva.Descricao, Is.EqualTo("Bateria não carrega"));
            Assert.That(ordemSalva.Situacao, Is.EqualTo(SituacaoOrdem.Concluida));
            Assert.That(ordemSalva.ValorFinal, Is.EqualTo(189.90m));
            Assert.That(ordemSalva.CriadaEm.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(ordemSalva.EncerradaEm.HasValue, Is.True);
            Assert.That(ordemSalva.EncerradaEm.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(ordemSalva.Versao.Length, Is.EqualTo(8));
            Assert.That(historico.Count, Is.EqualTo(3));
            Assert.That(historico.Last().Descricao, Does.Contain("Conector substituído e carga testada"));
            Assert.That(historico.Select(x => x.CriadoEm), Is.Ordered);
        }

        [Test]
        public void RowVersionRejeitaCopiaAntigaSemSalvarHistoricoDaTentativa()
        {
            var ordem = servico.AbrirOrdem(cliente.Id, "Monitor", "Imagem pisca ao ligar");
            var primeiraLeitura = repositorio.ObterOrdem(ordem.Id);
            var outraConexao = new RepositorioSqlServer(connectionString);
            var leituraAntiga = outraConexao.ObterOrdem(ordem.Id);
            var versaoOriginal = (byte[])leituraAntiga.Versao.Clone();
            primeiraLeitura.Situacao = SituacaoOrdem.EmAndamento;
            repositorio.SalvarAndamento(primeiraLeitura, "Primeiro atendente iniciou o reparo");

            leituraAntiga.Situacao = SituacaoOrdem.EmAndamento;
            Assert.Throws<ConflitoEdicaoException>(delegate
            {
                outraConexao.SalvarAndamento(leituraAntiga, "Mesmo início tentado por outro atendente");
            });

            leituraAntiga.Situacao = SituacaoOrdem.Cancelada;
            leituraAntiga.EncerradaEm = DateTime.UtcNow;
            Assert.Throws<ConflitoEdicaoException>(delegate
            {
                outraConexao.SalvarAndamento(leituraAntiga, "Tentativa antiga não deve entrar no histórico");
            });

            var salva = outraConexao.ObterOrdem(ordem.Id);
            var historico = outraConexao.ListarHistorico(ordem.Id);
            Assert.That(salva.Situacao, Is.EqualTo(SituacaoOrdem.EmAndamento));
            Assert.That(salva.Versao, Is.Not.EqualTo(versaoOriginal));
            Assert.That(salva.EncerradaEm, Is.Null);
            Assert.That(salva.ValorFinal, Is.Null);
            Assert.That(historico.Count, Is.EqualTo(2));
            Assert.That(historico.Last().Descricao, Is.EqualTo("Primeiro atendente iniciou o reparo"));
        }

        [Test]
        public void FiltroSqlTrataApostrofoPorcentagemESublinhadoComoTexto()
        {
            var primeira = servico.AbrirOrdem(cliente.Id, identificador + "%", "Bateria não carrega");
            var segunda = servico.AbrirOrdem(cliente.Id, identificador + "_", "Tela não acende");
            var terceira = servico.AbrirOrdem(cliente.Id, identificador + "X", "Fonte não liga");
            servico.IniciarOrdem(primeira.Id);

            var porCliente = servico.ListarOrdens(cliente.Nome, null);
            Assert.That(porCliente.Select(x => x.Id), Is.EqualTo(new[] { terceira.Id, segunda.Id, primeira.Id }));
            Assert.That(servico.ListarOrdens(identificador + "%", null).Single().Id,
                Is.EqualTo(primeira.Id));
            Assert.That(servico.ListarOrdens(identificador + "_", null).Single().Id,
                Is.EqualTo(segunda.Id));
            Assert.That(servico.ListarOrdens(cliente.Nome, SituacaoOrdem.EmAndamento).Single().Id,
                Is.EqualTo(primeira.Id));
            Assert.That(servico.ListarOrdens(identificador + "%", SituacaoOrdem.Aberta), Is.Empty);
        }
    }
}
