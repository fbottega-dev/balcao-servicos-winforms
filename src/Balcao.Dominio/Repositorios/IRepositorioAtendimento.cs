using System.Collections.Generic;
using Balcao.Dominio.Modelos;

namespace Balcao.Dominio.Repositorios
{
    public interface IRepositorioAtendimento
    {
        IList<Cliente> ListarClientes();
        Cliente ObterCliente(int id);
        IList<OrdemResumo> ListarOrdens(string busca, SituacaoOrdem? situacao);
        OrdemServico ObterOrdem(int id);
        void AdicionarCliente(Cliente cliente);
        void AtualizarCliente(Cliente cliente);
        void AdicionarOrdem(OrdemServico ordem);
        void SalvarAndamento(OrdemServico ordem, string descricaoHistorico);
        IList<HistoricoOrdem> ListarHistorico(int ordemId);
    }
}
