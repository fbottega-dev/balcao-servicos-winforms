namespace Balcao.Dominio.Modelos
{
    public enum SituacaoOrdem
    {
        Aberta = 0,
        EmAndamento = 1,
        Concluida = 2,
        Cancelada = 3
    }

    public static class Situacoes
    {
        public static string Descrever(SituacaoOrdem situacao)
        {
            switch (situacao)
            {
                case SituacaoOrdem.Aberta:
                    return "Aberta";
                case SituacaoOrdem.EmAndamento:
                    return "Em andamento";
                case SituacaoOrdem.Concluida:
                    return "Concluída";
                case SituacaoOrdem.Cancelada:
                    return "Cancelada";
                default:
                    return "Desconhecida";
            }
        }
    }
}
