using System;
using System.Drawing;

namespace Balcao.WinForms
{
    public sealed class CancelamentoForm : FormularioBase
    {
        public CancelamentoForm() : this(null) { }

        public CancelamentoForm(Func<string, bool> salvar)
            : base("Cancelar ordem", "A ordem será encerrada. Registre o motivo para futuras consultas.", new Size(560, 368))
        {
            var motivo = Tema.Entrada(300, true);
            motivo.Name = "motivo";
            AdicionarCampo("Motivo do cancelamento", motivo, 100);
            BotaoSalvar.Text = "Cancelar ordem";
            BotaoSalvar.BackColor = Tema.Erro;
            BotaoSalvar.FlatAppearance.BorderColor = Tema.Erro;
            BotaoSalvar.Click += delegate
            {
                if (salvar != null) FecharSeSalvo(salvar(motivo.Text));
            };
        }
    }
}
