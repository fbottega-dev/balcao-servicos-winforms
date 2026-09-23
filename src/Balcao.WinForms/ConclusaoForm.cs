using System;
using System.Drawing;
using System.Windows.Forms;

namespace Balcao.WinForms
{
    public sealed class ConclusaoForm : FormularioBase
    {
        public ConclusaoForm() : this(null) { }

        public ConclusaoForm(Func<decimal, string, bool> salvar)
            : base("Concluir atendimento", "O valor e o serviço realizado ficarão no histórico da ordem.", new Size(540, 430))
        {
            var valor = new NumericUpDown { Name = "valor", DecimalPlaces = 2, Maximum = 999999.99m, Minimum = 0, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right };
            var observacao = Tema.Entrada(300, true);
            observacao.Name = "observacao";
            AdicionarCampo("Valor final (R$)", valor, 28);
            AdicionarCampo("O que foi feito", observacao, 96);
            BotaoSalvar.Text = "Concluir";
            BotaoSalvar.Click += delegate
            {
                if (salvar != null) FecharSeSalvo(salvar(valor.Value, observacao.Text));
            };
        }
    }
}
