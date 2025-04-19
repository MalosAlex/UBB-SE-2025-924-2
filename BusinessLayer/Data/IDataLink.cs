using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BusinessLayer.Data
{
    public interface IDataLink : IDisposable
    {
        T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null);
        DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null);
        int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null);
        Task<DataTable> ExecuteReaderAsync(string storedProcedure, SqlParameter[]? sqlParameters = null);
        Task ExecuteNonQueryAsync(string storedProcedure, SqlParameter[]? sqlParameters = null);
    }
}