using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;


namespace Steam_Community.Data
{
    public class DatabaseConnectionSettings
    {
        public static string CONNECTION_STRING = "Integrated Security=True;TrustServerCertificate=True;data source=localhost\\SQLEXPRESS;initial catalog=iss;user id=sa";
    }
}
