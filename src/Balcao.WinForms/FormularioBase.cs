using System.Drawing;
using System.Windows.Forms;

namespace Balcao.WinForms
{
    public class FormularioBase : Form
    {
        protected readonly TableLayoutPanel Campos;
        protected readonly Button BotaoSalvar;
        protected readonly Button BotaoVoltar;

        public FormularioBase() : this("Cadastro", "Preencha os campos abaixo.", new Size(520, 400)) { }

        protected FormularioBase(string titulo, string orientacao, Size tamanho)
        {
            Text = titulo + " · Balcão de Serviços";
            Font = new Font("Segoe UI", 10);
            BackColor = Tema.Fundo;
            ForeColor = Tema.Texto;
            ClientSize = tamanho;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;

            var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
            estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            var cabecalho = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(24, 14, 24, 14), RowCount = 2, ColumnCount = 1, Margin = Padding.Empty };
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cabecalho.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            cabecalho.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cabecalho.Controls.Add(Tema.Rotulo(titulo, 18, Tema.Azul, true), 0, 0);
            cabecalho.Controls.Add(Tema.Rotulo(orientacao, 9, Tema.Secundario, false), 0, 1);
            estrutura.Controls.Add(cabecalho, 0, 0);

            var areaCampos = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 18, 24, 0), AutoScroll = true, Margin = Padding.Empty };
            Campos = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 0, Margin = Padding.Empty };
            Campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            areaCampos.Controls.Add(Campos);
            estrutura.Controls.Add(areaCampos, 0, 1);

            var rodape = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(24, 14, 14, 14), BackColor = Color.White, Margin = Padding.Empty };
            BotaoSalvar = Tema.Botao("Salvar", true);
            BotaoVoltar = Tema.Botao("Voltar", false);
            BotaoVoltar.DialogResult = DialogResult.Cancel;
            rodape.Controls.Add(BotaoSalvar);
            rodape.Controls.Add(BotaoVoltar);
            estrutura.Controls.Add(rodape, 0, 2);
            Controls.Add(estrutura);
            AcceptButton = BotaoSalvar;
            CancelButton = BotaoVoltar;
        }

        protected void AdicionarCampo(string titulo, Control controle, int altura)
        {
            int linha = Campos.RowCount;
            Campos.RowCount += 2;
            Campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            Campos.RowStyles.Add(new RowStyle(SizeType.Absolute, altura + 14));
            Campos.Controls.Add(Tema.Rotulo(titulo, 9, Tema.Texto, true), 0, linha);
            controle.Dock = DockStyle.Fill;
            controle.Margin = new Padding(0, 0, 0, 14);
            controle.AccessibleName = titulo;
            Campos.Controls.Add(controle, 0, linha + 1);
        }

        protected void FecharSeSalvo(bool salvo)
        {
            if (salvo) DialogResult = DialogResult.OK;
        }
    }
}
