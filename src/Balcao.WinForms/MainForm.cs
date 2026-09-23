using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Balcao.Apresentacao;
using Balcao.Dominio.Modelos;

namespace Balcao.WinForms
{
    public sealed class MainForm : Form, IAtendimentoView
    {
        private readonly TextBox busca = Tema.Entrada(100, false);
        private readonly ComboBox situacao = new ComboBox();
        private readonly DataGridView tabela = Tema.Tabela();
        private readonly Label abertas = Tema.Rotulo("0", 24, Tema.Azul, true);
        private readonly Label andamento = Tema.Rotulo("0", 24, Tema.Verde, true);
        private readonly Label encerradas = Tema.Rotulo("0", 24, Tema.Secundario, true);
        private readonly Label selecao = Tema.Rotulo("Selecione uma ordem para ver as ações disponíveis.", 9, Tema.Secundario, false);
        private readonly Label mensagem = Tema.Rotulo("Pronto para atender.", 9, Tema.Secundario, false);
        private readonly Button iniciar = Tema.Botao("Iniciar", false);
        private readonly Button concluir = Tema.Botao("Concluir", true);
        private readonly Button cancelar = Tema.Botao("Cancelar ordem", false);
        private readonly Button historico = Tema.Botao("Detalhes e histórico", false);

        public event EventHandler AtualizarSolicitado;
        public event EventHandler NovoClienteSolicitado;
        public event EventHandler NovaOrdemSolicitada;
        public event EventHandler IniciarSolicitado;
        public event EventHandler ConcluirSolicitado;
        public event EventHandler CancelarSolicitado;
        public event EventHandler HistoricoSolicitado;

        public MainForm() : this(false) { }

        public MainForm(bool modoDemo)
        {
            Text = "Balcão de Serviços" + (modoDemo ? " · Demonstração" : "");
            Font = new Font("Segoe UI", 10);
            BackColor = Tema.Fundo;
            ForeColor = Tema.Texto;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;

            var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
            estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            estrutura.Controls.Add(CriarCabecalho(modoDemo), 0, 0);

            var conteudo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(28, 18, 28, 12), Margin = Padding.Empty };
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 83));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 51));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            conteudo.Controls.Add(CriarTitulo(), 0, 0);
            conteudo.Controls.Add(CriarResumo(), 0, 1);
            conteudo.Controls.Add(CriarFiltros(), 0, 2);
            ConfigurarTabela();
            conteudo.Controls.Add(tabela, 0, 3);
            conteudo.Controls.Add(selecao, 0, 4);
            conteudo.Controls.Add(CriarAcoes(), 0, 5);
            conteudo.Controls.Add(mensagem, 0, 6);
            estrutura.Controls.Add(conteudo, 0, 1);
            Controls.Add(estrutura);
            AtualizarSelecao();
        }

        public string Busca { get { return busca.Text; } }

        public SituacaoOrdem? SituacaoSelecionada
        {
            get { return situacao.SelectedIndex <= 0 ? (SituacaoOrdem?)null : (SituacaoOrdem)(situacao.SelectedIndex - 1); }
        }

        public int? OrdemSelecionadaId
        {
            get
            {
                OrdemResumo ordem = OrdemSelecionada();
                return ordem == null ? (int?)null : ordem.Id;
            }
        }

        private Control CriarCabecalho(bool modoDemo)
        {
            var cabecalho = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Tema.Azul, Padding = new Padding(28, 18, 28, 18), ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
            cabecalho.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 316));
            var marca = Tema.Rotulo("BS", 20, Color.White, true);
            marca.BackColor = Tema.Verde;
            marca.TextAlign = ContentAlignment.MiddleCenter;
            marca.Margin = new Padding(0, 6, 10, 6);
            cabecalho.Controls.Add(marca, 0, 0);
            var texto = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
            texto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            texto.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            texto.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            texto.Controls.Add(Tema.Rotulo("Balcão de Serviços", 21, Color.White, true), 0, 0);
            texto.Controls.Add(Tema.Rotulo("Assistência técnica · Controle de atendimento", 10, Color.FromArgb(187, 207, 218), false), 0, 1);
            cabecalho.Controls.Add(texto, 1, 0);
            var ambiente = Tema.Rotulo(modoDemo ? "DEMONSTRAÇÃO\nDados de exemplo · não são salvos ao sair" : "ATENDIMENTO LOCAL\nDados salvos no SQL Server", 9, Color.FromArgb(193, 218, 222), false);
            ambiente.TextAlign = ContentAlignment.MiddleRight;
            cabecalho.Controls.Add(ambiente, 2, 0);
            return cabecalho;
        }

        private Control CriarTitulo()
        {
            var faixa = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            faixa.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            faixa.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            faixa.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            faixa.Controls.Add(Tema.Rotulo("Ordens de serviço", 21, Tema.Azul, true), 0, 0);
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0), Margin = Padding.Empty };
            var nova = Tema.Botao("+ Nova ordem", true);
            nova.Name = "novaOrdem";
            nova.Margin = new Padding(0, 0, 0, 0);
            nova.Click += delegate { Disparar(NovaOrdemSolicitada); };
            var cliente = Tema.Botao("Cadastrar cliente", false);
            cliente.Name = "cadastrarCliente";
            cliente.Width = 158;
            cliente.Click += delegate { Disparar(NovoClienteSolicitado); };
            botoes.Controls.Add(nova);
            botoes.Controls.Add(cliente);
            faixa.Controls.Add(botoes, 1, 0);
            return faixa;
        }

        private Control CriarResumo()
        {
            var resumo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty, Padding = new Padding(0, 4, 0, 0) };
            resumo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) resumo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            resumo.Controls.Add(CriarCartao("ABERTAS NA LISTA", abertas), 0, 0);
            resumo.Controls.Add(CriarCartao("EM ANDAMENTO NA LISTA", andamento), 1, 0);
            Control ultimo = CriarCartao("ENCERRADAS NA LISTA", encerradas);
            ultimo.Margin = Padding.Empty;
            resumo.Controls.Add(ultimo, 2, 0);
            return resumo;
        }

        private static Control CriarCartao(string titulo, Label quantidade)
        {
            var cartao = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, ColumnCount = 1, RowCount = 2, Padding = new Padding(16, 8, 16, 6), Margin = new Padding(0, 0, 12, 0) };
            cartao.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cartao.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            cartao.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cartao.Controls.Add(Tema.Rotulo(titulo, 8.5f, Tema.Secundario, true), 0, 0);
            cartao.Controls.Add(quantidade, 0, 1);
            return cartao;
        }

        private Control CriarFiltros()
        {
            var filtros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0, 10, 0, 10), Margin = Padding.Empty };
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 206));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98));
            filtros.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            filtros.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            filtros.Controls.Add(Tema.Rotulo("Buscar por cliente ou equipamento", 9, Tema.Secundario, true), 0, 0);
            filtros.Controls.Add(Tema.Rotulo("Situação", 9, Tema.Secundario, true), 1, 0);
            busca.Name = "busca";
            busca.AccessibleName = "Buscar por cliente ou equipamento";
            busca.Margin = new Padding(0, 0, 14, 0);
            busca.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { Disparar(AtualizarSolicitado); e.SuppressKeyPress = true; }
            };
            filtros.Controls.Add(busca, 0, 1);
            situacao.Name = "situacao";
            situacao.AccessibleName = "Filtrar por situação";
            situacao.DropDownStyle = ComboBoxStyle.DropDownList;
            situacao.Dock = DockStyle.Fill;
            situacao.Margin = new Padding(0, 0, 14, 0);
            situacao.Items.AddRange(new object[] { "Todas", "Aberta", "Em andamento", "Concluída", "Cancelada" });
            situacao.SelectedIndex = 0;
            filtros.Controls.Add(situacao, 1, 1);
            var filtrar = Tema.Botao("Filtrar", true);
            filtrar.Dock = DockStyle.Fill;
            filtrar.Click += delegate { Disparar(AtualizarSolicitado); };
            filtros.Controls.Add(filtrar, 2, 1);
            var limpar = Tema.Botao("Limpar", false);
            limpar.Dock = DockStyle.Fill;
            limpar.Margin = Padding.Empty;
            limpar.Click += delegate { busca.Clear(); situacao.SelectedIndex = 0; Disparar(AtualizarSolicitado); };
            filtros.Controls.Add(limpar, 3, 1);
            return filtros;
        }

        private void ConfigurarTabela()
        {
            tabela.Name = "ordens";
            tabela.AccessibleName = "Ordens de serviço";
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ORDEM", FillWeight = 10, MinimumWidth = 80 });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClienteNome", HeaderText = "CLIENTE", FillWeight = 23 });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Equipamento", HeaderText = "EQUIPAMENTO", FillWeight = 26 });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SituacaoTexto", HeaderText = "SITUAÇÃO", Name = "SituacaoTexto", FillWeight = 18 });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CriadaEm", HeaderText = "ENTRADA", FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            tabela.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ValorFinal", HeaderText = "VALOR", FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", FormatProvider = CultureInfo.GetCultureInfo("pt-BR"), NullValue = "—", Alignment = DataGridViewContentAlignment.MiddleRight } });
            tabela.SelectionChanged += delegate { AtualizarSelecao(); };
            tabela.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) Disparar(HistoricoSolicitado); };
            tabela.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.RowIndex >= 0 && tabela.Columns[e.ColumnIndex].DataPropertyName == "CriadaEm" && e.Value is DateTime)
                {
                    e.Value = ((DateTime)e.Value).ToLocalTime().ToString("dd/MM/yyyy");
                    e.FormattingApplied = true;
                    return;
                }
                if (e.RowIndex < 0 || tabela.Columns[e.ColumnIndex].Name != "SituacaoTexto") return;
                var ordem = tabela.Rows[e.RowIndex].DataBoundItem as OrdemResumo;
                if (ordem == null) return;
                e.CellStyle.ForeColor = ordem.Situacao == SituacaoOrdem.Cancelada ? Tema.Erro : ordem.Situacao == SituacaoOrdem.EmAndamento ? Tema.Verde : Tema.Texto;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            };
        }

        private Control CriarAcoes()
        {
            var acoes = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 3, 0, 0), Margin = Padding.Empty, WrapContents = false };
            iniciar.Name = "iniciar";
            concluir.Name = "concluir";
            cancelar.Name = "cancelar";
            historico.Name = "historico";
            historico.Width = 184;
            iniciar.Click += delegate { Disparar(IniciarSolicitado); };
            concluir.Click += delegate { Disparar(ConcluirSolicitado); };
            cancelar.Click += delegate { Disparar(CancelarSolicitado); };
            historico.Click += delegate { Disparar(HistoricoSolicitado); };
            acoes.Controls.AddRange(new Control[] { iniciar, concluir, cancelar, historico });
            return acoes;
        }

        private OrdemResumo OrdemSelecionada()
        {
            return tabela.CurrentRow == null ? null : tabela.CurrentRow.DataBoundItem as OrdemResumo;
        }

        private void AtualizarSelecao()
        {
            OrdemResumo ordem = OrdemSelecionada();
            iniciar.Enabled = ordem != null && ordem.Situacao == SituacaoOrdem.Aberta;
            concluir.Enabled = ordem != null && ordem.Situacao == SituacaoOrdem.EmAndamento;
            cancelar.Enabled = ordem != null && (ordem.Situacao == SituacaoOrdem.Aberta || ordem.Situacao == SituacaoOrdem.EmAndamento);
            historico.Enabled = ordem != null;
            selecao.Text = ordem == null ? "Nenhuma ordem selecionada." : "Selecionada: #" + ordem.Id + " · " + ordem.ClienteNome + " · " + ordem.Equipamento;
        }

        public void ExibirOrdens(IList<OrdemResumo> ordens)
        {
            int? idAnterior = OrdemSelecionadaId;
            tabela.DataSource = null;
            tabela.DataSource = ordens;
            int totalAbertas = 0, totalAndamento = 0, totalEncerradas = 0;
            foreach (OrdemResumo ordem in ordens)
            {
                if (ordem.Situacao == SituacaoOrdem.Aberta) totalAbertas++;
                else if (ordem.Situacao == SituacaoOrdem.EmAndamento) totalAndamento++;
                else totalEncerradas++;
            }
            abertas.Text = totalAbertas.ToString();
            andamento.Text = totalAndamento.ToString();
            encerradas.Text = totalEncerradas.ToString();
            foreach (DataGridViewRow linha in tabela.Rows)
            {
                var ordem = linha.DataBoundItem as OrdemResumo;
                if (ordem != null && idAnterior.HasValue && ordem.Id == idAnterior.Value)
                {
                    tabela.CurrentCell = linha.Cells[0];
                    break;
                }
            }
            AtualizarSelecao();
            if (ordens.Count == 0)
                ExibirMensagem("Nenhuma ordem encontrada. Ajuste os filtros ou abra uma nova ordem.");
            else if (ordens.Count == 500)
                ExibirMensagem("Mostrando as 500 ordens mais recentes. Refine os filtros para localizar outras ordens.");
            else
                ExibirMensagem(ordens.Count + " ordem(ns) na lista. Use os filtros para localizar um atendimento.");
        }

        public void ExibirMensagem(string texto)
        {
            mensagem.ForeColor = Tema.Secundario;
            mensagem.Text = texto;
        }

        public void ExibirErro(string texto)
        {
            mensagem.ForeColor = Tema.Erro;
            mensagem.Text = texto;
            if (OwnedForms.Length > 0)
                MessageBox.Show(OwnedForms[OwnedForms.Length - 1], texto, "Confira os dados", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ExibirCadastroCliente(Func<string, string, bool> salvar)
        {
            using (var formulario = new ClienteForm(salvar)) formulario.ShowDialog(this);
        }

        public void ExibirNovaOrdem(IList<Cliente> clientes, Func<int, string, string, bool> salvar)
        {
            using (var formulario = new NovaOrdemForm(clientes, salvar)) formulario.ShowDialog(this);
        }

        public void ExibirConclusao(Func<decimal, string, bool> salvar)
        {
            using (var formulario = new ConclusaoForm(salvar)) formulario.ShowDialog(this);
        }

        public void ExibirCancelamento(Func<string, bool> salvar)
        {
            using (var formulario = new CancelamentoForm(salvar)) formulario.ShowDialog(this);
        }

        public void ExibirHistorico(OrdemServico ordem, IList<HistoricoOrdem> registros)
        {
            using (var formulario = new HistoricoForm(ordem, registros)) formulario.ShowDialog(this);
        }

        private void Disparar(EventHandler evento)
        {
            if (evento != null) evento(this, EventArgs.Empty);
        }
    }
}
