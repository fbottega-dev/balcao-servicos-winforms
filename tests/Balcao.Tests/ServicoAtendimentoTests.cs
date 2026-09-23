using System;
using System.Linq;
using Balcao.Dados;
using Balcao.Dominio;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Servicos;
using NUnit.Framework;

namespace Balcao.Tests
{
    [TestFixture]
    public class ServicoAtendimentoTests
    {
        private RepositorioMemoria repositorio;
        private ServicoAtendimento servico;

        [SetUp]
        public void Preparar()
        {
            repositorio = new RepositorioMemoria();
            servico = new ServicoAtendimento(repositorio);
        }

        [Test]
        public void CadastroPreservaAcentosERetiraEspacosNasPontas()
        {
            var cliente = servico.CadastrarCliente("  João D'Ávila  ", "  (41) 99999-0000  ");

            var salvo = servico.ListarClientes().Single();
            Assert.That(salvo.Id, Is.EqualTo(cliente.Id).And.GreaterThan(0));
            Assert.That(salvo.Nome, Is.EqualTo("João D'Ávila"));
            Assert.That(salvo.Telefone, Is.EqualTo("(41) 99999-0000"));
        }

        [Test]
        public void NomeEmBrancoNaoCriaCliente()
        {
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.CadastrarCliente("   ", "41999990000");
            });

            Assert.That(servico.ListarClientes(), Is.Empty);
        }

        [Test]
        public void ClienteInexistenteNaoPodeReceberOrdem()
        {
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.AbrirOrdem(999, "Notebook", "Bateria não carrega");
            });

            Assert.That(servico.ListarOrdens(null, null), Is.Empty);
        }

        [Test]
        public void AtendimentoPrecisaSerIniciadoAntesDaConclusao()
        {
            var ordem = AbrirOrdem();
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.ConcluirOrdem(ordem.Id, 125.90m, "Fonte substituída");
            });
            Assert.That(servico.ListarHistorico(ordem.Id).Count, Is.EqualTo(1));

            servico.IniciarOrdem(ordem.Id);
            servico.ConcluirOrdem(ordem.Id, 125.90m, "Fonte substituída");

            var salva = repositorio.ObterOrdem(ordem.Id);
            Assert.That(salva.Situacao, Is.EqualTo(SituacaoOrdem.Concluida));
            Assert.That(salva.ValorFinal, Is.EqualTo(125.90m));
            Assert.That(salva.EncerradaEm, Is.Not.Null);
            Assert.That(salva.EncerradaEm.Value, Is.GreaterThanOrEqualTo(salva.CriadaEm));
            var historico = servico.ListarHistorico(ordem.Id);
            Assert.That(historico.Count, Is.EqualTo(3));
            Assert.That(historico.Last().Descricao, Does.Contain("Fonte substituída"));
        }

        [Test]
        public void ServicoEmGarantiaPodeSerConcluidoSemCobranca()
        {
            var ordem = AbrirOrdem();
            servico.IniciarOrdem(ordem.Id);

            servico.ConcluirOrdem(ordem.Id, 0m, "Reparo coberto pela garantia");

            Assert.That(repositorio.ObterOrdem(ordem.Id).ValorFinal, Is.EqualTo(0m));
            Assert.That(repositorio.ObterOrdem(ordem.Id).Situacao, Is.EqualTo(SituacaoOrdem.Concluida));
        }

        [TestCase(-1)]
        [TestCase(0.001)]
        [TestCase(1000000)]
        public void ValorInvalidoNaoEncerraOrdemNemAcrescentaHistorico(decimal valor)
        {
            var ordem = AbrirOrdem();
            servico.IniciarOrdem(ordem.Id);
            var antes = repositorio.ObterOrdem(ordem.Id);

            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.ConcluirOrdem(ordem.Id, valor, "Fonte substituída");
            });

            var depois = repositorio.ObterOrdem(ordem.Id);
            Assert.That(depois.Situacao, Is.EqualTo(SituacaoOrdem.EmAndamento));
            Assert.That(depois.ValorFinal, Is.Null);
            Assert.That(depois.EncerradaEm, Is.Null);
            Assert.That(depois.Versao, Is.EqualTo(antes.Versao));
            Assert.That(servico.ListarHistorico(ordem.Id).Count, Is.EqualTo(2));
        }

        [Test]
        public void CancelamentoExigeMotivoERegistraOTextoInformado()
        {
            var ordem = AbrirOrdem();
            var antes = repositorio.ObterOrdem(ordem.Id);
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.CancelarOrdem(ordem.Id, "  ");
            });
            Assert.That(repositorio.ObterOrdem(ordem.Id).Versao, Is.EqualTo(antes.Versao));
            Assert.That(servico.ListarHistorico(ordem.Id).Count, Is.EqualTo(1));

            servico.CancelarOrdem(ordem.Id, "  Cliente não aprovou o orçamento  ");

            var salva = repositorio.ObterOrdem(ordem.Id);
            Assert.That(salva.Situacao, Is.EqualTo(SituacaoOrdem.Cancelada));
            Assert.That(salva.EncerradaEm, Is.Not.Null);
            Assert.That(salva.ValorFinal, Is.Null);
            Assert.That(servico.ListarHistorico(ordem.Id).Last().Descricao,
                Does.Contain("Cliente não aprovou o orçamento"));
        }

        [TestCase(SituacaoOrdem.Concluida)]
        [TestCase(SituacaoOrdem.Cancelada)]
        public void OrdemEncerradaNaoAceitaOutraTransicao(SituacaoOrdem situacao)
        {
            var ordem = AbrirOrdem();
            if (situacao == SituacaoOrdem.Concluida)
            {
                servico.IniciarOrdem(ordem.Id);
                servico.ConcluirOrdem(ordem.Id, 80m, "Conector substituído");
            }
            else
            {
                servico.CancelarOrdem(ordem.Id, "Cliente retirou o equipamento");
            }
            var antes = repositorio.ObterOrdem(ordem.Id);
            var quantidadeHistorico = servico.ListarHistorico(ordem.Id).Count;

            Assert.Throws<RegraNegocioException>(delegate { servico.IniciarOrdem(ordem.Id); });
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.ConcluirOrdem(ordem.Id, 90m, "Tentativa de concluir novamente");
            });
            Assert.Throws<RegraNegocioException>(delegate
            {
                servico.CancelarOrdem(ordem.Id, "Tentativa de cancelar novamente");
            });

            var depois = repositorio.ObterOrdem(ordem.Id);
            Assert.That(depois.Situacao, Is.EqualTo(antes.Situacao));
            Assert.That(depois.ValorFinal, Is.EqualTo(antes.ValorFinal));
            Assert.That(depois.EncerradaEm, Is.EqualTo(antes.EncerradaEm));
            Assert.That(depois.Versao, Is.EqualTo(antes.Versao));
            Assert.That(servico.ListarHistorico(ordem.Id).Count, Is.EqualTo(quantidadeHistorico));
        }

        [Test]
        public void BuscaCombinaSituacaoClienteETextoLiteralEOrdenaMaisRecentesPrimeiro()
        {
            var cliente = servico.CadastrarCliente("João D'Ávila", "41999990000");
            var primeira = servico.AbrirOrdem(cliente.Id, "Notebook", "Bateria para em 50%");
            var segunda = servico.AbrirOrdem(cliente.Id, "Monitor", "Imagem não aparece");
            servico.IniciarOrdem(primeira.Id);

            var porCliente = servico.ListarOrdens("  JOÃO D'ÁVILA  ", null);
            Assert.That(porCliente.Select(x => x.Id), Is.EqualTo(new[] { segunda.Id, primeira.Id }));
            Assert.That(servico.ListarOrdens("%", SituacaoOrdem.EmAndamento).Single().Id,
                Is.EqualTo(primeira.Id));
            Assert.That(servico.ListarOrdens("%", SituacaoOrdem.Aberta), Is.Empty);
            Assert.That(servico.ListarOrdens("MONITOR", SituacaoOrdem.Aberta).Single().Id,
                Is.EqualTo(segunda.Id));
        }

        [Test]
        public void CopiaAntigaNaoSobrescreveEstadoNemAcrescentaHistorico()
        {
            var ordem = AbrirOrdem();
            var primeiraLeitura = repositorio.ObterOrdem(ordem.Id);
            var leituraAntiga = repositorio.ObterOrdem(ordem.Id);
            primeiraLeitura.Situacao = SituacaoOrdem.EmAndamento;
            repositorio.SalvarAndamento(primeiraLeitura, "Primeiro atendente iniciou o reparo");

            leituraAntiga.Situacao = SituacaoOrdem.EmAndamento;
            Assert.Throws<ConflitoEdicaoException>(delegate
            {
                repositorio.SalvarAndamento(leituraAntiga, "Segundo atendente tentou iniciar também");
            });

            leituraAntiga.Situacao = SituacaoOrdem.Cancelada;
            leituraAntiga.EncerradaEm = DateTime.UtcNow;
            Assert.Throws<ConflitoEdicaoException>(delegate
            {
                repositorio.SalvarAndamento(leituraAntiga, "Segundo atendente tentou cancelar");
            });

            var salva = repositorio.ObterOrdem(ordem.Id);
            Assert.That(salva.Situacao, Is.EqualTo(SituacaoOrdem.EmAndamento));
            Assert.That(salva.EncerradaEm, Is.Null);
            var historico = servico.ListarHistorico(ordem.Id);
            Assert.That(historico.Count, Is.EqualTo(2));
            Assert.That(historico.Last().Descricao, Is.EqualTo("Primeiro atendente iniciou o reparo"));
        }

        private OrdemServico AbrirOrdem()
        {
            var cliente = servico.CadastrarCliente("Marina Souza", "41999990000");
            return servico.AbrirOrdem(cliente.Id, "Notebook", "Bateria não carrega");
        }
    }
}
