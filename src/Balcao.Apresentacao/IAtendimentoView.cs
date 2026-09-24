using System;
using System.Collections.Generic;
using Balcao.Dominio.Modelos;

namespace Balcao.Apresentacao
{
    public interface IAtendimentoView
    {
        string Busca { get; }
        SituacaoOrdem? SituacaoSelecionada { get; }
        int? OrdemSelecionadaId { get; }

        event EventHandler AtualizarSolicitado;
        event EventHandler NovoClienteSolicitado;
        event EventHandler EditarClienteSolicitado;
        event EventHandler NovaOrdemSolicitada;
        event EventHandler IniciarSolicitado;
        event EventHandler ConcluirSolicitado;
        event EventHandler CancelarSolicitado;
        event EventHandler HistoricoSolicitado;

        void ExibirOrdens(IList<OrdemResumo> ordens);
        void ExibirMensagem(string mensagem);
        void ExibirErro(string mensagem);
        void ExibirCadastroCliente(Func<string, string, bool> salvar);
        void ExibirEdicaoCliente(Cliente cliente, Func<string, string, bool> salvar);
        void ExibirNovaOrdem(IList<Cliente> clientes, Func<int, string, string, bool> salvar);
        void ExibirConclusao(Func<decimal, string, bool> salvar);
        void ExibirCancelamento(Func<string, bool> salvar);
        void ExibirHistorico(OrdemServico ordem, IList<HistoricoOrdem> historico);
    }
}
