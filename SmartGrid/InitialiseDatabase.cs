using Npgsql;

namespace SmartGrid
{
    internal class InitialiseDatabase
    {
        public void CreateDatabase()
        {
            string connStr = "Server=localhost;Port=5432;User Id=postgres;Password=enter;";
            var m_conn = new NpgsqlConnection(connStr);
            var m_createdb_cmd = new NpgsqlCommand(@"
                CREATE DATABASE IF NOT EXISTS testDb
                WITH OWNER = postgres
                ENCODING = 'UTF8'
                CONNECTION LIMIT = -1;
                ", m_conn);
            m_conn.Open();
            m_createdb_cmd.ExecuteNonQuery();
            m_conn.Close();

            connStr = "Server=localhost;Port=5432;User Id=postgres;Password=enter;Database=testDb";
            m_conn = new NpgsqlConnection(connStr);
            var m_createtbl_cmd = new NpgsqlCommand(
                  "CREATE TABLE table1(ID CHAR(256) CONSTRAINT id PRIMARY KEY, Title CHAR)"
                  , m_conn);
            m_conn.Open();
            m_createtbl_cmd.ExecuteNonQuery();
            m_conn.Close();
        }

        public void PopulateDatabase(SmartGrid smartGrid)
        {
        }
    }
}