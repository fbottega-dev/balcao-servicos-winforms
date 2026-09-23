using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Balcao.Dominio.Modelos;

namespace Balcao.WinForms
{
    public sealed class NovaOrdemForm : FormularioBase
    {
        public NovaOrdemForm() : this(new List<Cliente>(), null) { }

        public NovaOrdemForm(IList<Cliente> clientes, Func<int, string, string, bool> salvar)
            : base("Abrir ordem de serviço", "Registre o equipamento e o problema informado pelo cliente.", new Size(570, 496))
        {
            var cliente = new ComboBox { Name = "cliente", DropDownStyle = ComboBoxStyle.DropDownList, DataSource = clientes, DisplayMember = "Nome", ValueMember = "Id" };
            var equipamento = Tema.Entrada(80, false);
            var descricao = Tema.Entrada(500, true);
            equipamento.Name = "equipamento";
            descricao.Name = "descricao";
            AdicionarCampo("Cliente", cliente, 28);
            AdicionarCampo("Equipamento", equipamento, 28);
            AdicionarCampo("Problema informado", descricao, 98);
            BotaoSalvar.Text = "Abrir ordem";
            BotaoSalvar.Click += delegate
            {
                var selecionado = cliente.SelectedItem as Cliente;
                if (salvar != null) FecharSeSalvo(salvar(selecionado == null ? 0 : selecionado.Id, equipamento.Text, descricao.Text));
            };
        }
    }
}
