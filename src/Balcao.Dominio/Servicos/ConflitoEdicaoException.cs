using System;

namespace Balcao.Dominio
{
    public class ConflitoEdicaoException : Exception
    {
        public ConflitoEdicaoException(string mensagem) : base(mensagem)
        {
        }

        public ConflitoEdicaoException(string mensagem, Exception causa) : base(mensagem, causa)
        {
        }
    }
}
