using System;
using Balcao.Dominio;
using Balcao.Dominio.Servicos;

namespace Balcao.Apresentacao
{
    public sealed class AtendimentoPresenter : IDisposable
    {
        private readonly IAtendimentoView view;
        private readonly ServicoAtendimento servico;

        public AtendimentoPresenter(IAtendimentoView view, ServicoAtendimento servico)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (servico == null) throw new ArgumentNullException("servico");
            this.view = view;
            this.servico = servico;
            view.AtualizarSolicitado += AoAtualizar;
            view.NovoClienteSolicitado += AoCadastrarCliente;
            view.NovaOrdemSolicitada += AoAbrirOrdem;
            view.IniciarSolicitado += AoIniciar;
            view.ConcluirSolicitado += AoConcluir;
            view.CancelarSolicitado += AoCancelar;
            view.HistoricoSolicitado += AoExibirHistorico;
        }

        public void Atualizar()
        {
            Executar(delegate { view.ExibirOrdens(servico.ListarOrdens(view.Busca, view.SituacaoSelecionada)); });
        }

        private void AoAtualizar(object sender, EventArgs e) { Atualizar(); }

        private void AoCadastrarCliente(object sender, EventArgs e)
        {
            view.ExibirCadastroCliente(delegate(string nome, string telefone)
            {
                bool salvo = Executar(delegate { servico.CadastrarCliente(nome, telefone); });
                if (salvo) view.ExibirMensagem("Cliente cadastrado. Você já pode abrir uma ordem para ele.");
                return salvo;
            });
        }

        private void AoAbrirOrdem(object sender, EventArgs e)
        {
            Executar(delegate
            {
                var clientes = servico.ListarClientes();
                if (clientes.Count == 0)
                {
                    view.ExibirMensagem("Cadastre um cliente antes de abrir a primeira ordem.");
                    return;
                }
                view.ExibirNovaOrdem(clientes, delegate(int clienteId, string equipamento, string descricao)
                {
                    bool salvo = Executar(delegate { servico.AbrirOrdem(clienteId, equipamento, descricao); });
                    if (salvo)
                    {
                        Atualizar();
                        view.ExibirMensagem("Ordem aberta. Selecione a linha para iniciar o atendimento.");
                    }
                    return salvo;
                });
            });
        }

        private int? ObterSelecao()
        {
            int? id = view.OrdemSelecionadaId;
            if (!id.HasValue) view.ExibirMensagem("Selecione uma ordem na lista primeiro.");
            return id;
        }

        private void AoIniciar(object sender, EventArgs e)
        {
            int? id = ObterSelecao();
            if (!id.HasValue) return;
            if (Executar(delegate { servico.IniciarOrdem(id.Value); }))
            {
                Atualizar();
                view.ExibirMensagem("Atendimento iniciado.");
            }
        }

        private void AoConcluir(object sender, EventArgs e)
        {
            int? id = ObterSelecao();
            if (!id.HasValue) return;
            view.ExibirConclusao(delegate(decimal valor, string observacao)
            {
                bool salvo = Executar(delegate { servico.ConcluirOrdem(id.Value, valor, observacao); });
                if (salvo)
                {
                    Atualizar();
                    view.ExibirMensagem("Ordem concluída. O valor e a observação foram registrados.");
                }
                return salvo;
            });
        }

        private void AoCancelar(object sender, EventArgs e)
        {
            int? id = ObterSelecao();
            if (!id.HasValue) return;
            view.ExibirCancelamento(delegate(string motivo)
            {
                bool salvo = Executar(delegate { servico.CancelarOrdem(id.Value, motivo); });
                if (salvo)
                {
                    Atualizar();
                    view.ExibirMensagem("Ordem cancelada. O motivo ficou salvo no histórico.");
                }
                return salvo;
            });
        }

        private void AoExibirHistorico(object sender, EventArgs e)
        {
            int? id = ObterSelecao();
            if (id.HasValue)
                Executar(delegate { view.ExibirHistorico(servico.ObterOrdem(id.Value), servico.ListarHistorico(id.Value)); });
        }

        private bool Executar(Action acao)
        {
            try
            {
                acao();
                return true;
            }
            catch (RegraNegocioException erro)
            {
                view.ExibirErro(erro.Message);
            }
            catch (ConflitoEdicaoException erro)
            {
                Atualizar();
                view.ExibirErro(erro.Message);
            }
            catch (Exception)
            {
                // Mensagens do provedor podem conter detalhes da conexão.
                view.ExibirErro("Não foi possível completar a operação. Confira a conexão com o banco e tente novamente.");
            }
            return false;
        }

        public void Dispose()
        {
            view.AtualizarSolicitado -= AoAtualizar;
            view.NovoClienteSolicitado -= AoCadastrarCliente;
            view.NovaOrdemSolicitada -= AoAbrirOrdem;
            view.IniciarSolicitado -= AoIniciar;
            view.ConcluirSolicitado -= AoConcluir;
            view.CancelarSolicitado -= AoCancelar;
            view.HistoricoSolicitado -= AoExibirHistorico;
        }
    }
}
