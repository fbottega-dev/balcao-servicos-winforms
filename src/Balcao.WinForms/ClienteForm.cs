using System;
using System.Drawing;
using Balcao.Dominio.Modelos;

namespace Balcao.WinForms
{
    public sealed class ClienteForm : FormularioBase
    {
        public ClienteForm() : this(null) { }

        public ClienteForm(Func<string, string, bool> salvar) : this(null, salvar) { }

        public ClienteForm(Cliente cliente, Func<string, string, bool> salvar)
            : base(cliente == null ? "Cadastrar cliente" : "Editar cliente",
                cliente == null ? "Dados de contato para acompanhar o atendimento." : "A alteração vale para todas as ordens deste cliente.",
                new Size(510, 362))
        {
            var nome = Tema.Entrada(80, false);
            var telefone = Tema.Entrada(20, false);
            nome.Name = "nome";
            telefone.Name = "telefone";
            if (cliente != null)
            {
                nome.Text = cliente.Nome;
                telefone.Text = cliente.Telefone;
            }
            AdicionarCampo("Nome do cliente", nome, 28);
            AdicionarCampo("Telefone com DDD (opcional)", telefone, 28);
            BotaoSalvar.Text = cliente == null ? "Cadastrar" : "Salvar alterações";
            BotaoSalvar.Click += delegate
            {
                if (salvar != null) FecharSeSalvo(salvar(nome.Text, telefone.Text));
            };
        }
    }
}
