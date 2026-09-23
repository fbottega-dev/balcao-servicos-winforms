using System.Drawing;
using System.Windows.Forms;

namespace Balcao.WinForms
{
    internal static class Tema
    {
        internal static readonly Color Fundo = Color.FromArgb(244, 247, 249);
        internal static readonly Color Azul = Color.FromArgb(25, 45, 62);
        internal static readonly Color Verde = Color.FromArgb(0, 112, 104);
        internal static readonly Color Texto = Color.FromArgb(37, 52, 65);
        internal static readonly Color Secundario = Color.FromArgb(96, 112, 126);
        internal static readonly Color Borda = Color.FromArgb(214, 223, 230);
        internal static readonly Color Erro = Color.FromArgb(164, 47, 52);

        internal static Label Rotulo(string texto, float tamanho, Color cor, bool negrito)
        {
            return new Label
            {
                Text = texto,
                AutoSize = false,
                ForeColor = cor,
                Font = new Font("Segoe UI", tamanho, negrito ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
        }

        internal static Button Botao(string texto, bool principal)
        {
            var botao = new Button
            {
                Text = texto,
                AutoSize = false,
                Size = new Size(138, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = principal ? Verde : Color.White,
                ForeColor = principal ? Color.White : Texto,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0),
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderColor = principal ? Verde : Borda;
            botao.FlatAppearance.BorderSize = 1;
            botao.EnabledChanged += delegate
            {
                botao.BackColor = botao.Enabled ? (principal ? Verde : Color.White) : Color.FromArgb(229, 234, 238);
                botao.ForeColor = botao.Enabled ? (principal ? Color.White : Texto) : Secundario;
                botao.FlatAppearance.BorderColor = botao.Enabled && principal ? Verde : Borda;
            };
            return botao;
        }

        internal static TextBox Entrada(int limite, bool variasLinhas)
        {
            return new TextBox
            {
                MaxLength = limite,
                Multiline = variasLinhas,
                ScrollBars = variasLinhas ? ScrollBars.Vertical : ScrollBars.None,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14)
            };
        }

        internal static DataGridView Tabela()
        {
            var tabela = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(236, 240, 243),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = Padding.Empty
            };
            tabela.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(233, 239, 243),
                ForeColor = Secundario,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(10, 0, 5, 0),
                SelectionBackColor = Color.FromArgb(233, 239, 243),
                SelectionForeColor = Secundario
            };
            tabela.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(10, 0, 5, 0),
                SelectionBackColor = Color.FromArgb(220, 240, 237),
                SelectionForeColor = Azul
            };
            tabela.RowTemplate.Height = 46;
            return tabela;
        }
    }
}
