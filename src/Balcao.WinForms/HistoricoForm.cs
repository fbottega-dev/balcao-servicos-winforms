using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Balcao.Dominio.Modelos;

namespace Balcao.WinForms
{
    public sealed class HistoricoForm : FormularioBase
    {
        public HistoricoForm() : this(new OrdemServico(), new List<HistoricoOrdem>()) { }

        public HistoricoForm(OrdemServico ordem, IList<HistoricoOrdem> historico)
            : base("Ordem #" + ordem.Id + " · " + ordem.SituacaoTexto, ordem.Equipamento, new Size(780, 650))
        {
            var descricao = Tema.Entrada(500, true);
            descricao.Name = "problemaInformado";
            descricao.Text = ordem.Descricao;
            descricao.ReadOnly = true;
            descricao.BackColor = Color.White;
            AdicionarCampo("Problema informado pelo cliente", descricao, 110);

            var tabela = Tema.Tabela();
            tabela.Name = "historicoDaOrdem";
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CriadoEm", HeaderText = "DATA", FillWeight = 28, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descricao", HeaderText = "REGISTRO", FillWeight = 72, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
            tabela.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            tabela.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            tabela.DataBindingComplete += delegate { tabela.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders); };
            tabela.ColumnWidthChanged += delegate { tabela.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders); };
            tabela.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 0 && e.Value is DateTime)
                {
                    e.Value = ((DateTime)e.Value).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                    e.FormattingApplied = true;
                }
            };
            tabela.DataSource = historico;
            AdicionarCampo("Histórico do atendimento", tabela, 244);
            BotaoSalvar.Text = "Fechar";
            BotaoVoltar.Visible = false;
            CancelButton = BotaoSalvar;
            BotaoSalvar.Click += delegate { DialogResult = DialogResult.OK; };
        }
    }
}
