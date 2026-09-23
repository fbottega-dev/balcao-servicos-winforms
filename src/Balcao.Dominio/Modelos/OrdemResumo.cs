using System;

namespace Balcao.Dominio.Modelos
{
    public sealed class OrdemResumo
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; }
        public string Equipamento { get; set; }
        public SituacaoOrdem Situacao { get; set; }
        public decimal? ValorFinal { get; set; }
        public DateTime CriadaEm { get; set; }

        public string SituacaoTexto
        {
            get { return Situacoes.Descrever(Situacao); }
        }
    }
}
