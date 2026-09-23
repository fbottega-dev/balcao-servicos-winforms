using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Balcao.Apresentacao;
using Balcao.Dados;
using Balcao.Dominio.Repositorios;
using Balcao.Dominio.Servicos;

namespace Balcao.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool modoDemo = Array.Exists(args, argumento => string.Equals(argumento, "--demo", StringComparison.OrdinalIgnoreCase));
            IRepositorioAtendimento repositorio;
            try
            {
                repositorio = CriarRepositorio(modoDemo);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Confira a conexão Atendimento no arquivo de configuração ou a variável BALCAO_CONNECTION_STRING. Para conhecer as telas sem banco, use iniciar-demo.cmd.",
                    "Configuração do Balcão de Serviços", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var servico = new ServicoAtendimento(repositorio);
            if (modoDemo) CarregarExemplo(servico);

            using (var tela = new MainForm(modoDemo))
            using (var presenter = new AtendimentoPresenter(tela, servico))
            {
                tela.Shown += delegate { presenter.Atualizar(); };
                Application.Run(tela);
            }
        }

        private static IRepositorioAtendimento CriarRepositorio(bool modoDemo)
        {
            if (modoDemo) return new RepositorioMemoria();

            string conexao = Environment.GetEnvironmentVariable("BALCAO_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(conexao))
            {
                var configuracao = ConfigurationManager.ConnectionStrings["Atendimento"];
                if (configuracao == null || string.IsNullOrWhiteSpace(configuracao.ConnectionString))
                    throw new ConfigurationErrorsException("A conexão Atendimento não foi configurada.");
                conexao = configuracao.ConnectionString;
            }
            return new RepositorioSqlServer(conexao);
        }

        private static void CarregarExemplo(ServicoAtendimento servico)
        {
            var marina = servico.CadastrarCliente("Marina Alves", "(41) 99910-2040");
            var paulo = servico.CadastrarCliente("Paulo Nunes", "(41) 99820-5060");
            var oficina = servico.CadastrarCliente("Oficina do Bairro", "(41) 3333-1020");

            var impressora = servico.AbrirOrdem(oficina.Id, "Impressora Epson L3150", "Papel prende na entrada durante a impressão.");
            servico.IniciarOrdem(impressora.Id);
            servico.ConcluirOrdem(impressora.Id, 120m, "Limpeza dos roletes e teste de impressão realizados.");

            var monitor = servico.AbrirOrdem(paulo.Id, "Monitor LG 24 polegadas", "A tela apaga depois de alguns minutos de uso.");
            servico.CancelarOrdem(monitor.Id, "Cliente optou por retirar o equipamento sem reparo.");

            var computador = servico.AbrirOrdem(oficina.Id, "Computador do caixa", "Computador reinicia ao abrir o sistema do caixa.");
            servico.IniciarOrdem(computador.Id);

            var notebook = servico.AbrirOrdem(marina.Id, "Notebook Lenovo Ideapad", "Bateria não carrega mesmo com a fonte conectada.");
            servico.IniciarOrdem(notebook.Id);

            servico.AbrirOrdem(paulo.Id, "Notebook Dell Inspiron", "Teclado falha nas teclas A, S e D.");
            servico.AbrirOrdem(marina.Id, "Tablet Samsung A8", "Conector de carga apresenta mau contato.");
        }
    }
}
