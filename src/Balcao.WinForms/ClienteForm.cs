using System;
using System.Drawing;

namespace Balcao.WinForms
{
    public sealed class ClienteForm : FormularioBase
    {
        public ClienteForm() : this(null) { }

        public ClienteForm(Func<string, string, bool> salvar)
            : base("Cadastrar cliente", "Dados de contato para acompanhar o atendimento.", new Size(510, 362))
        {
            var nome = Tema.Entrada(80, false);
            var telefone = Tema.Entrada(20, false);
            nome.Name = "nome";
            telefone.Name = "telefone";
            AdicionarCampo("Nome do cliente", nome, 28);
            AdicionarCampo("Telefone com DDD (opcional)", telefone, 28);
            BotaoSalvar.Text = "Cadastrar";
            BotaoSalvar.Click += delegate
            {
                if (salvar != null) FecharSeSalvo(salvar(nome.Text, telefone.Text));
            };
        }
    }
}
