using System.Data.Entity;
using Balcao.Dominio.Modelos;

namespace Balcao.Dados
{
    [DbConfigurationType(typeof(ConfiguracaoEf))]
    public sealed class AtendimentoContext : DbContext
    {
        static AtendimentoContext()
        {
            Database.SetInitializer<AtendimentoContext>(null);
        }

        public AtendimentoContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<OrdemServico> Ordens { get; set; }
        public DbSet<HistoricoOrdem> Historicos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var cliente = modelBuilder.Entity<Cliente>();
            cliente.ToTable("Clientes", "dbo");
            cliente.HasKey(item => item.Id);
            cliente.Property(item => item.Nome).IsRequired().HasMaxLength(80);
            cliente.Property(item => item.Telefone).IsRequired().HasMaxLength(20);

            var ordem = modelBuilder.Entity<OrdemServico>();
            ordem.ToTable("OrdensServico", "dbo");
            ordem.HasKey(item => item.Id);
            ordem.Property(item => item.Equipamento).IsRequired().HasMaxLength(80);
            ordem.Property(item => item.Descricao).IsRequired().HasMaxLength(500);
            ordem.Property(item => item.ValorFinal).HasPrecision(8, 2);
            ordem.Property(item => item.CriadaEm).HasColumnType("datetime2");
            ordem.Property(item => item.EncerradaEm).HasColumnType("datetime2");
            ordem.Property(item => item.Versao).IsRowVersion();
            ordem.Ignore(item => item.SituacaoTexto);

            var historico = modelBuilder.Entity<HistoricoOrdem>();
            historico.ToTable("HistoricosOrdem", "dbo");
            historico.HasKey(item => item.Id);
            historico.Property(item => item.Descricao).IsRequired().HasMaxLength(400);
            historico.Property(item => item.CriadoEm).HasColumnType("datetime2");
            base.OnModelCreating(modelBuilder);
        }
    }
}
