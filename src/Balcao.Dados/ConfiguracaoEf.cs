using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace Balcao.Dados
{
    public sealed class ConfiguracaoEf : DbConfiguration
    {
        public ConfiguracaoEf()
        {
            SetProviderFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
        }
    }
}
