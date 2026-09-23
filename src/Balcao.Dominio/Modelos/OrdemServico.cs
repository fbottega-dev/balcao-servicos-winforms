using System;

namespace Balcao.Dominio.Modelos
{
    public sealed class OrdemServico
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Equipamento { get; set; }
        public string Descricao { get; set; }
        public SituacaoOrdem Situacao { get; set; }
        public decimal? ValorFinal { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? EncerradaEm { get; set; }
        public byte[] Versao { get; set; }

        public string SituacaoTexto
        {
            get { return Situacoes.Descrever(Situacao); }
        }
    }
}
