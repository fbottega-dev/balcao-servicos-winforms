using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Balcao.Dominio.Modelos;
using Balcao.Dominio.Repositorios;

namespace Balcao.Dominio.Servicos
{
    public sealed class ServicoAtendimento
    {
        private readonly IRepositorioAtendimento repositorio;

        public ServicoAtendimento(IRepositorioAtendimento repositorio)
        {
            if (repositorio == null)
                throw new ArgumentNullException("repositorio");

            this.repositorio = repositorio;
        }

        public IList<Cliente> ListarClientes()
        {
            return repositorio.ListarClientes();
        }

        public IList<OrdemResumo> ListarOrdens(string busca, SituacaoOrdem? situacao)
        {
            busca = (busca ?? string.Empty).Trim();
            if (busca.Length > 100)
                throw new RegraNegocioException("Use até 100 caracteres na busca.");
            if (situacao.HasValue && !Enum.IsDefined(typeof(SituacaoOrdem), situacao.Value))
                throw new RegraNegocioException("Escolha uma situação válida.");

            return repositorio.ListarOrdens(busca, situacao);
        }

        public OrdemServico ObterOrdem(int ordemId)
        {
            return ObterOrdemExistente(ordemId);
        }

        public IList<HistoricoOrdem> ListarHistorico(int ordemId)
        {
            ObterOrdemExistente(ordemId);
            return repositorio.ListarHistorico(ordemId);
        }

        public Cliente CadastrarCliente(string nome, string telefone)
        {
            nome = ValidarTexto(nome, "O nome", 2, 80);
            telefone = (telefone ?? string.Empty).Trim();
            if (telefone.Length > 20)
                throw new RegraNegocioException("O telefone deve ter até 20 caracteres.");

            var cliente = new Cliente { Nome = nome, Telefone = telefone };
            repositorio.AdicionarCliente(cliente);
            return cliente;
        }

        public OrdemServico AbrirOrdem(int clienteId, string equipamento, string descricao)
        {
            if (!repositorio.ListarClientes().Any(cliente => cliente.Id == clienteId))
                throw new RegraNegocioException("Selecione um cliente cadastrado.");

            equipamento = ValidarTexto(equipamento, "O equipamento", 2, 80);
            descricao = ValidarTexto(descricao, "A descrição", 5, 500);

            var ordem = new OrdemServico
            {
                ClienteId = clienteId,
                Equipamento = equipamento,
                Descricao = descricao,
                Situacao = SituacaoOrdem.Aberta,
                CriadaEm = DateTime.UtcNow
            };
            repositorio.AdicionarOrdem(ordem);
            return ordem;
        }

        public void IniciarOrdem(int ordemId)
        {
            var ordem = ObterOrdemExistente(ordemId);
            if (ordem.Situacao != SituacaoOrdem.Aberta)
                throw new RegraNegocioException("Só é possível iniciar uma ordem aberta.");

            ordem.Situacao = SituacaoOrdem.EmAndamento;
            repositorio.SalvarAndamento(ordem, "Atendimento iniciado.");
        }

        public void ConcluirOrdem(int ordemId, decimal valor, string observacao)
        {
            observacao = ValidarTexto(observacao, "A observação", 5, 300);
            if (valor < 0 || valor > 999999.99m || decimal.Round(valor, 2) != valor)
                throw new RegraNegocioException("Informe um valor de 0 a 999.999,99, com até duas casas decimais.");

            var ordem = ObterOrdemExistente(ordemId);
            if (ordem.Situacao != SituacaoOrdem.EmAndamento)
                throw new RegraNegocioException("Inicie o atendimento antes de concluir a ordem.");

            ordem.Situacao = SituacaoOrdem.Concluida;
            ordem.ValorFinal = valor;
            ordem.EncerradaEm = DateTime.UtcNow;
            var descricao = "Atendimento concluído por " + valor.ToString("C2", CultureInfo.GetCultureInfo("pt-BR")) + ". " + observacao;
            repositorio.SalvarAndamento(ordem, descricao);
        }

        public void CancelarOrdem(int ordemId, string motivo)
        {
            motivo = ValidarTexto(motivo, "O motivo", 5, 300);
            var ordem = ObterOrdemExistente(ordemId);
            if (ordem.Situacao != SituacaoOrdem.Aberta && ordem.Situacao != SituacaoOrdem.EmAndamento)
                throw new RegraNegocioException("Uma ordem encerrada não pode ser cancelada.");

            ordem.Situacao = SituacaoOrdem.Cancelada;
            ordem.EncerradaEm = DateTime.UtcNow;
            repositorio.SalvarAndamento(ordem, "Ordem cancelada. Motivo: " + motivo);
        }

        private OrdemServico ObterOrdemExistente(int id)
        {
            var ordem = repositorio.ObterOrdem(id);
            if (ordem == null)
                throw new RegraNegocioException("A ordem não foi encontrada. Atualize a lista e tente novamente.");
            return ordem;
        }

        private static string ValidarTexto(string texto, string campo, int minimo, int maximo)
        {
            texto = (texto ?? string.Empty).Trim();
            if (texto.Length < minimo || texto.Length > maximo)
                throw new RegraNegocioException(campo + " deve ter de " + minimo + " a " + maximo + " caracteres.");
            return texto;
        }
    }
}
