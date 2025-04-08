using System.Data;
using Microsoft.Data.SqlClient;

namespace dotnet_api_extensions.Sql
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();

        void RollBack();

        void SetIsolationLevel(IsolationLevel isolationLevel);

        SqlTransaction Transaction { get; }

        SqlConnection Connection { get; }
    }
}
