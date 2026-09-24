using System;
using System.Collections.Generic;
using System.Linq;
using Balcao.Apresentacao;
using Balcao.Dados;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Servicos;
using NUnit.Framework;

namespace Balcao.Tests
{
    [TestFixture]
    public class AtendimentoPresenterTests
    {
        private RepositorioMemoria repositorio;
        private ServicoAtendimento servico;
        private TelaFalsa tela;
        private AtendimentoPresenter presenter;

        [SetUp]
        public void Preparar()
        {
            repositorio = new RepositorioMemoria();
            servico = new ServicoAtendimento(repositorio);
            tela = new TelaFalsa();
            presenter = new AtendimentoPresenter(tela, servico);
        }

        [TearDown]
        public void Liberar()
        {
            presenter.Dispose();
        }

        [Test]
        public void AtualizarAplicaOsFiltrosDaTela()
        {
            var cliente = servico.CadastrarCliente("Joana Lima", "41999990000");
            var impressora = servico.AbrirOrdem(cliente.Id, "Impressora", "Papel fica preso");
            servico.AbrirOrdem(cliente.Id, "Notebook", "Tela não acende");
            servico.IniciarOrdem(impressora.Id);
            tela.Busca = "impressora";
            tela.SituacaoSelecionada = SituacaoOrdem.EmAndamento;

            tela.SolicitarAtualizacao();

            Assert.That(tela.Ordens.Count, Is.EqualTo(1));
            Assert.That(tela.Ordens[0].Id, Is.EqualTo(impressora.Id));
            Assert.That(tela.Erro, Is.Null);
        }

        [Test]
        public void IniciarExigeSelecaoEAtualizaAListaAposSalvar()
        {
            var cliente = servico.CadastrarCliente("Bruno Costa", "41999990000");
            var ordem = servico.AbrirOrdem(cliente.Id, "Monitor", "Imagem pisca");

            tela.SolicitarInicio();

            Assert.That(tela.Mensagem, Is.Not.Null.And.Not.Empty);
            Assert.That(servico.ListarOrdens(null, null)[0].Situacao,
                Is.EqualTo(SituacaoOrdem.Aberta));

            tela.Erro = null;
            tela.OrdemSelecionadaId = ordem.Id;
            tela.SolicitarInicio();

            Assert.That(tela.Erro, Is.Null);
            Assert.That(tela.Ordens[0].Situacao, Is.EqualTo(SituacaoOrdem.EmAndamento));
        }

        [Test]
        public void CadastroInvalidoMantemFormularioAbertoEPermiteCorrigir()
        {
            tela.SolicitarCadastroCliente();
            Assert.That(tela.SalvarCliente, Is.Not.Null);

            bool fecharComErro = tela.SalvarCliente("   ", "41999990000");

            Assert.That(fecharComErro, Is.False);
            Assert.That(tela.Erro, Is.Not.Null.And.Not.Empty);
            Assert.That(servico.ListarClientes(), Is.Empty);

            tela.Erro = null;
            bool fecharComSucesso = tela.SalvarCliente("Marina Souza", "41999990000");

            Assert.That(fecharComSucesso, Is.True);
            Assert.That(tela.Erro, Is.Null);
            Assert.That(servico.ListarClientes().Count, Is.EqualTo(1));
            Assert.That(servico.ListarClientes()[0].Nome, Is.EqualTo("Marina Souza"));
        }

        [Test]
        public void DetalhesMostramProblemaOriginalMesmoDepoisDeConcluir()
        {
            var cliente = servico.CadastrarCliente("Joana Lima", "");
            var ordem = servico.AbrirOrdem(cliente.Id, "Notebook", "Desliga quando sai da tomada");
            servico.IniciarOrdem(ordem.Id);
            servico.ConcluirOrdem(ordem.Id, 180m, "Bateria substituída e testada");
            tela.OrdemSelecionadaId = ordem.Id;

            tela.SolicitarHistorico();

            Assert.That(tela.Detalhes.Descricao, Is.EqualTo("Desliga quando sai da tomada"));
            Assert.That(tela.Detalhes.Situacao, Is.EqualTo(SituacaoOrdem.Concluida));
            Assert.That(tela.Historico.Count, Is.EqualTo(3));
            Assert.That(tela.Erro, Is.Null);
        }

        [Test]
        public void EditarClienteDaOrdemAtualizaTodasAsListagens()
        {
            var cliente = servico.CadastrarCliente("Marina Costa", "41990000000");
            var primeira = servico.AbrirOrdem(cliente.Id, "Notebook", "Bateria não carrega");
            servico.AbrirOrdem(cliente.Id, "Monitor", "Imagem fica piscando");
            tela.OrdemSelecionadaId = primeira.Id;
            tela.SolicitarEdicaoCliente();

            Assert.That(tela.ClienteEmEdicao.Nome, Is.EqualTo("Marina Costa"));
            Assert.That(tela.ClienteEmEdicao.Telefone, Is.EqualTo("41990000000"));
            Assert.That(tela.SalvarEdicao("Marina Souza", "41991112222"), Is.True);
            Assert.That(tela.Ordens.Count, Is.EqualTo(2));
            Assert.That(tela.Ordens.All(x => x.ClienteNome == "Marina Souza"), Is.True);
            Assert.That(servico.ObterCliente(cliente.Id).Telefone, Is.EqualTo("41991112222"));
        }

        [Test]
        public void EdicaoInvalidaPermiteCorrigirSemAlterarCliente()
        {
            var cliente = servico.CadastrarCliente("Marina Costa", "41990000000");
            var ordem = servico.AbrirOrdem(cliente.Id, "Notebook", "Bateria não carrega");
            tela.OrdemSelecionadaId = ordem.Id;
            tela.SolicitarEdicaoCliente();

            Assert.That(tela.SalvarEdicao(" ", "41990000000"), Is.False);
            Assert.That(servico.ObterCliente(cliente.Id).Nome, Is.EqualTo("Marina Costa"));
            Assert.That(tela.SalvarEdicao("Marina Souza", ""), Is.True);
            Assert.That(servico.ObterCliente(cliente.Id).Nome, Is.EqualTo("Marina Souza"));
        }

        [Test]
        public void EditarClienteExigeOrdemSelecionada()
        {
            tela.SolicitarEdicaoCliente();

            Assert.That(tela.SalvarEdicao, Is.Null);
            Assert.That(tela.Mensagem, Does.Contain("Selecione"));
        }

        [Test]
        public void DisposeDesligaEventosDaTela()
        {
            presenter.Dispose();

            tela.SolicitarAtualizacao();
            tela.SolicitarCadastroCliente();
            tela.SolicitarEdicaoCliente();
            tela.SolicitarInicio();

            Assert.That(tela.Atualizacoes, Is.Zero);
            Assert.That(tela.SalvarCliente, Is.Null);
            Assert.That(tela.SalvarEdicao, Is.Null);
            Assert.That(tela.Erro, Is.Null);
        }

        private sealed class TelaFalsa : IAtendimentoView
        {
            public string Busca { get; set; }
            public SituacaoOrdem? SituacaoSelecionada { get; set; }
            public int? OrdemSelecionadaId { get; set; }
            public IList<OrdemResumo> Ordens { get; private set; }
            public string Erro { get; set; }
            public string Mensagem { get; private set; }
            public int Atualizacoes { get; private set; }
            public Func<string, string, bool> SalvarCliente { get; private set; }
            public Func<string, string, bool> SalvarEdicao { get; private set; }
            public Cliente ClienteEmEdicao { get; private set; }
            public OrdemServico Detalhes { get; private set; }
            public IList<HistoricoOrdem> Historico { get; private set; }

            public event EventHandler AtualizarSolicitado;
            public event EventHandler NovoClienteSolicitado;
            public event EventHandler EditarClienteSolicitado;
            public event EventHandler NovaOrdemSolicitada;
            public event EventHandler IniciarSolicitado;
            public event EventHandler ConcluirSolicitado;
            public event EventHandler CancelarSolicitado;
            public event EventHandler HistoricoSolicitado;

            public void ExibirOrdens(IList<OrdemResumo> ordens)
            {
                Ordens = ordens;
                Atualizacoes++;
            }

            public void ExibirMensagem(string mensagem) { Mensagem = mensagem; }
            public void ExibirErro(string mensagem) { Erro = mensagem; }
            public void ExibirCadastroCliente(Func<string, string, bool> salvar)
            {
                SalvarCliente = salvar;
            }
            public void ExibirEdicaoCliente(Cliente cliente, Func<string, string, bool> salvar)
            {
                ClienteEmEdicao = cliente;
                SalvarEdicao = salvar;
            }
            public void ExibirNovaOrdem(IList<Cliente> clientes, Func<int, string, string, bool> salvar) { }
            public void ExibirConclusao(Func<decimal, string, bool> salvar) { }
            public void ExibirCancelamento(Func<string, bool> salvar) { }
            public void ExibirHistorico(OrdemServico ordem, IList<HistoricoOrdem> historico)
            {
                Detalhes = ordem;
                Historico = historico;
            }

            public void SolicitarAtualizacao() { Disparar(AtualizarSolicitado); }
            public void SolicitarCadastroCliente() { Disparar(NovoClienteSolicitado); }
            public void SolicitarEdicaoCliente() { Disparar(EditarClienteSolicitado); }
            public void SolicitarInicio() { Disparar(IniciarSolicitado); }
            public void SolicitarHistorico() { Disparar(HistoricoSolicitado); }

            private void Disparar(EventHandler evento)
            {
                if (evento != null)
                    evento(this, EventArgs.Empty);
            }
        }
    }
}
