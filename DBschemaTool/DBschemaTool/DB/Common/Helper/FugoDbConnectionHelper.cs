
using DBschemaTool.Repository.Common.Interface;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DBschemaTool.Repository.Common.Helper
{
    public class DbConnectionHelper : IDatabaseConnectionHelper
    {
        private readonly string _connectionString;
        private readonly string _providerName;

        public DbConnectionHelper(string connect)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[connect].ConnectionString;
            _providerName = ConfigurationManager.ConnectionStrings[connect].ProviderName;
        }

        /// <summary>
        /// Create DbConnection
        /// </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            if (_providerName.StartsWith("Oracle"))
                return new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString); //new OracleConnection(_connectionString);// SqlConnection(_connectionString);
            else
            {
                return new SqlConnection(_connectionString);
            }

        }

    }
}
