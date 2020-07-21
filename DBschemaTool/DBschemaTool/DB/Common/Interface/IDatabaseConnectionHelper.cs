using System.Data;

namespace DBschemaTool.Repository.Common.Interface
{
    public interface IDatabaseConnectionHelper
    {
        IDbConnection Create();
    }
}
