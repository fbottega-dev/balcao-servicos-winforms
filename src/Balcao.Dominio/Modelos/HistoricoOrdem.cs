using System;

namespace Balcao.Dominio.Modelos
{
    public sealed class HistoricoOrdem
    {
        public int Id { get; set; }
        public int OrdemId { get; set; }
        public string Descricao { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
