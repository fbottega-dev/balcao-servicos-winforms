using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Balcao.Dominio;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Repositorios;

namespace Balcao.Dados
{
    public sealed class RepositorioSqlServer : IRepositorioAtendimento
    {
        private readonly string connectionString;

        public RepositorioSqlServer(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Informe a conexão com o SQL Server.", "connectionString");
            this.connectionString = connectionString;
        }

        public IList<Cliente> ListarClientes()
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                return banco.Clientes.AsNoTracking().OrderBy(cliente => cliente.Nome).ThenBy(cliente => cliente.Id).ToList();
            }
        }

        public Cliente ObterCliente(int id)
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                return banco.Clientes.AsNoTracking().SingleOrDefault(item => item.Id == id);
            }
        }

        public IList<OrdemResumo> ListarOrdens(string busca, SituacaoOrdem? situacao)
        {
            busca = (busca ?? string.Empty).Trim();
            using (var banco = new AtendimentoContext(connectionString))
            {
                var consulta = from ordem in banco.Ordens.AsNoTracking()
                               join cliente in banco.Clientes.AsNoTracking() on ordem.ClienteId equals cliente.Id
                               select new { Ordem = ordem, Cliente = cliente };
                if (busca.Length > 0)
                    consulta = consulta.Where(item => item.Cliente.Nome.Contains(busca) || item.Ordem.Equipamento.Contains(busca) || item.Ordem.Descricao.Contains(busca));
                if (situacao.HasValue)
                {
                    var filtro = situacao.Value;
                    consulta = consulta.Where(item => item.Ordem.Situacao == filtro);
                }

                var resultado = consulta.OrderByDescending(item => item.Ordem.CriadaEm).ThenByDescending(item => item.Ordem.Id).Take(500)
                    .Select(item => new OrdemResumo
                    {
                        Id = item.Ordem.Id,
                        ClienteNome = item.Cliente.Nome,
                        Equipamento = item.Ordem.Equipamento,
                        Situacao = item.Ordem.Situacao,
                        ValorFinal = item.Ordem.ValorFinal,
                        CriadaEm = item.Ordem.CriadaEm
                    }).ToList();

                foreach (var ordem in resultado)
                    ordem.CriadaEm = DateTime.SpecifyKind(ordem.CriadaEm, DateTimeKind.Utc);
                return resultado;
            }
        }

        public OrdemServico ObterOrdem(int id)
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                var ordem = banco.Ordens.AsNoTracking().SingleOrDefault(item => item.Id == id);
                if (ordem != null)
                {
                    ordem.CriadaEm = DateTime.SpecifyKind(ordem.CriadaEm, DateTimeKind.Utc);
                    if (ordem.EncerradaEm.HasValue)
                        ordem.EncerradaEm = DateTime.SpecifyKind(ordem.EncerradaEm.Value, DateTimeKind.Utc);
                }
                return ordem;
            }
        }

        public void AdicionarCliente(Cliente cliente)
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                banco.Clientes.Add(cliente);
                banco.SaveChanges();
            }
        }

        public void AtualizarCliente(Cliente cliente)
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                var registro = banco.Clientes.SingleOrDefault(item => item.Id == cliente.Id);
                if (registro == null)
                    throw new RegraNegocioException("O cliente não foi encontrado. Atualize a lista e tente novamente.");
                registro.Nome = cliente.Nome;
                registro.Telefone = cliente.Telefone;
                banco.SaveChanges();
            }
        }

        public void AdicionarOrdem(OrdemServico ordem)
        {
            using (var banco = new AtendimentoContext(connectionString))
            using (var transacao = banco.Database.BeginTransaction())
            {
                banco.Ordens.Add(ordem);
                banco.SaveChanges();
                banco.Historicos.Add(new HistoricoOrdem
                {
                    OrdemId = ordem.Id,
                    Descricao = "Ordem aberta.",
                    CriadoEm = ordem.CriadaEm
                });
                banco.SaveChanges();
                transacao.Commit();
            }
        }

        public void SalvarAndamento(OrdemServico ordem, string descricaoHistorico)
        {
            if (ordem.Versao == null || ordem.Versao.Length != 8)
                throw new ConflitoEdicaoException("A versão da ordem não está disponível. Atualize a lista.");

            using (var banco = new AtendimentoContext(connectionString))
            {
                var registro = banco.Ordens.SingleOrDefault(item => item.Id == ordem.Id);
                if (registro == null)
                    throw new ConflitoEdicaoException("A ordem não está mais disponível. Atualize a lista.");

                registro.Situacao = ordem.Situacao;
                registro.ValorFinal = ordem.ValorFinal;
                registro.EncerradaEm = ordem.EncerradaEm;
                banco.Entry(registro).Property(item => item.Versao).OriginalValue = ordem.Versao;
                banco.Entry(registro).Property(item => item.Situacao).IsModified = true;
                banco.Historicos.Add(new HistoricoOrdem
                {
                    OrdemId = ordem.Id,
                    Descricao = descricaoHistorico,
                    CriadoEm = DateTime.UtcNow
                });

                try
                {
                    // O EF inclui a versão no UPDATE e salva o histórico na mesma transação.
                    banco.SaveChanges();
                    ordem.Versao = (byte[])registro.Versao.Clone();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new ConflitoEdicaoException("Outra pessoa alterou esta ordem. Atualize a lista e tente novamente.", ex);
                }
            }
        }

        public IList<HistoricoOrdem> ListarHistorico(int ordemId)
        {
            using (var banco = new AtendimentoContext(connectionString))
            {
                var resultado = banco.Historicos.AsNoTracking().Where(item => item.OrdemId == ordemId)
                    .OrderBy(item => item.CriadoEm).ThenBy(item => item.Id).ToList();
                foreach (var historico in resultado)
                    historico.CriadoEm = DateTime.SpecifyKind(historico.CriadoEm, DateTimeKind.Utc);
                return resultado;
            }
        }
    }
}
