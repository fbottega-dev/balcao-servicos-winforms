using System;
using System.Collections.Generic;
using System.Linq;
using Balcao.Dominio;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Repositorios;

namespace Balcao.Dados
{
    // Usado pelo modo demonstração. Cada instância começa vazia e não grava arquivos.
    public sealed class RepositorioMemoria : IRepositorioAtendimento
    {
        private readonly object trava = new object();
        private readonly List<Cliente> clientes = new List<Cliente>();
        private readonly List<OrdemServico> ordens = new List<OrdemServico>();
        private readonly List<HistoricoOrdem> historicos = new List<HistoricoOrdem>();
        private long versaoAtual;

        public IList<Cliente> ListarClientes()
        {
            lock (trava)
            {
                return clientes.OrderBy(cliente => cliente.Nome).ThenBy(cliente => cliente.Id).Select(CopiarCliente).ToList();
            }
        }

        public Cliente ObterCliente(int id)
        {
            lock (trava)
            {
                var cliente = clientes.SingleOrDefault(item => item.Id == id);
                return cliente == null ? null : CopiarCliente(cliente);
            }
        }

        public IList<OrdemResumo> ListarOrdens(string busca, SituacaoOrdem? situacao)
        {
            busca = (busca ?? string.Empty).Trim();
            lock (trava)
            {
                var consulta = from ordem in ordens
                               join cliente in clientes on ordem.ClienteId equals cliente.Id
                               where (!situacao.HasValue || ordem.Situacao == situacao.Value)
                                   && (Contem(cliente.Nome, busca) || Contem(ordem.Equipamento, busca) || Contem(ordem.Descricao, busca))
                               orderby ordem.CriadaEm descending, ordem.Id descending
                               select new OrdemResumo
                               {
                                   Id = ordem.Id,
                                   ClienteNome = cliente.Nome,
                                   Equipamento = ordem.Equipamento,
                                   Situacao = ordem.Situacao,
                                   ValorFinal = ordem.ValorFinal,
                                   CriadaEm = ordem.CriadaEm
                               };
                return consulta.Take(500).ToList();
            }
        }

        public OrdemServico ObterOrdem(int id)
        {
            lock (trava)
            {
                var ordem = ordens.SingleOrDefault(item => item.Id == id);
                return ordem == null ? null : CopiarOrdem(ordem);
            }
        }

        public void AdicionarCliente(Cliente cliente)
        {
            lock (trava)
            {
                cliente.Id = clientes.Count + 1;
                clientes.Add(CopiarCliente(cliente));
            }
        }

        public void AtualizarCliente(Cliente cliente)
        {
            lock (trava)
            {
                var registro = clientes.SingleOrDefault(item => item.Id == cliente.Id);
                if (registro == null)
                    throw new RegraNegocioException("O cliente não foi encontrado. Atualize a lista e tente novamente.");
                registro.Nome = cliente.Nome;
                registro.Telefone = cliente.Telefone;
            }
        }

        public void AdicionarOrdem(OrdemServico ordem)
        {
            lock (trava)
            {
                if (!clientes.Any(cliente => cliente.Id == ordem.ClienteId))
                    throw new RegraNegocioException("Selecione um cliente cadastrado.");

                ordem.Id = ordens.Count + 1;
                ordem.Versao = BitConverter.GetBytes(++versaoAtual);
                ordens.Add(CopiarOrdem(ordem));
                historicos.Add(new HistoricoOrdem
                {
                    Id = historicos.Count + 1,
                    OrdemId = ordem.Id,
                    Descricao = "Ordem aberta.",
                    CriadoEm = ordem.CriadaEm
                });
            }
        }

        public void SalvarAndamento(OrdemServico ordem, string descricaoHistorico)
        {
            lock (trava)
            {
                var registro = ordens.SingleOrDefault(item => item.Id == ordem.Id);
                if (registro == null || ordem.Versao == null || !registro.Versao.SequenceEqual(ordem.Versao))
                    throw new ConflitoEdicaoException("Outra pessoa alterou esta ordem. Atualize a lista e tente novamente.");

                registro.Situacao = ordem.Situacao;
                registro.ValorFinal = ordem.ValorFinal;
                registro.EncerradaEm = ordem.EncerradaEm;
                registro.Versao = BitConverter.GetBytes(++versaoAtual);
                ordem.Versao = (byte[])registro.Versao.Clone();
                historicos.Add(new HistoricoOrdem
                {
                    Id = historicos.Count + 1,
                    OrdemId = ordem.Id,
                    Descricao = descricaoHistorico,
                    CriadoEm = DateTime.UtcNow
                });
            }
        }

        public IList<HistoricoOrdem> ListarHistorico(int ordemId)
        {
            lock (trava)
            {
                return historicos.Where(item => item.OrdemId == ordemId).OrderBy(item => item.CriadoEm).ThenBy(item => item.Id)
                    .Select(item => new HistoricoOrdem
                    {
                        Id = item.Id,
                        OrdemId = item.OrdemId,
                        Descricao = item.Descricao,
                        CriadoEm = item.CriadoEm
                    }).ToList();
            }
        }

        private static bool Contem(string texto, string busca)
        {
            return texto.IndexOf(busca, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Cliente CopiarCliente(Cliente cliente)
        {
            return new Cliente { Id = cliente.Id, Nome = cliente.Nome, Telefone = cliente.Telefone };
        }

        private static OrdemServico CopiarOrdem(OrdemServico ordem)
        {
            return new OrdemServico
            {
                Id = ordem.Id,
                ClienteId = ordem.ClienteId,
                Equipamento = ordem.Equipamento,
                Descricao = ordem.Descricao,
                Situacao = ordem.Situacao,
                ValorFinal = ordem.ValorFinal,
                CriadaEm = ordem.CriadaEm,
                EncerradaEm = ordem.EncerradaEm,
                Versao = (byte[])ordem.Versao.Clone()
            };
        }
    }
}
