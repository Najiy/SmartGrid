using System.Linq;
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
            var connString = "Host=127.0.0.1;Username=postgres;Password=;Database=SmartGrid";

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                foreach (var node in smartGrid.RoadNodes)
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO RoadNodes (ID, Latitude, Longitude) VALUES (@p, @q, @s)";
                        cmd.Parameters.AddWithValue("p", $"'{node.Key}'");
                        cmd.Parameters.AddWithValue("q", node.Value.Coordinate.Latitude);
                        cmd.Parameters.AddWithValue("s", node.Value.Coordinate.Longitude);

                        cmd.ExecuteNonQuery();
                    }
                    
                    if (node.Value.Descriptors == null) continue;
                    foreach (var descriptor in node.Value.Descriptors)
                    {
                         var descriptorCmd = new NpgsqlCommand();
                        descriptorCmd.Connection = conn;
                        descriptorCmd.CommandText =
                            "INSERT INTO descriptors (id, attribute, value) VALUES (@i, @a, @v)";
                        descriptorCmd.Parameters.AddWithValue("i", node.Key);
                        descriptorCmd.Parameters.AddWithValue("a", descriptor.Key);

                        descriptorCmd.Parameters.AddWithValue("v", descriptor.Value ?? "");
                        descriptorCmd.ExecuteNonQuery();
                    }

                }
                foreach (var link in smartGrid.RoadLinks)
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO Roadlinks (ID, roadvectors) VALUES (@i, @r)";
                        cmd.Parameters.AddWithValue("i", $"'{link.Key}'");
                        //List<string> managerList = matchedManager.Select(m => m.FullName).ToList();
                        var vectors = link.Value.Vectors.Select(x => x.NodeFrom.ToString() + " " + x.NodeTo.ToString());
                        cmd.Parameters.AddWithValue("r", vectors.ToArray());


                        cmd.ExecuteNonQuery();
                    }
                  
                    
                    if (link.Value.Descriptors == null) continue;
                    foreach (var descriptor in link.Value.Descriptors)
                    {
                        var descriptorCmd = new NpgsqlCommand();
                        descriptorCmd.Connection = conn;
                        descriptorCmd.CommandText =
                            "INSERT INTO descriptors (attribute, value, id) VALUES (@a, @v, @i)";
                        descriptorCmd.Parameters.AddWithValue("i", link.Key);
                        descriptorCmd.Parameters.AddWithValue("a", descriptor.Key);
                        descriptorCmd.Parameters.AddWithValue("v", descriptor.Value ?? "");
                        descriptorCmd.ExecuteNonQuery();
                    }



                }
            }



        }
    }
}