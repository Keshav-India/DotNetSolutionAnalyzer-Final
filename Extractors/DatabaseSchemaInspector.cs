using System.Data.SqlClient;

namespace DotNetSolutionAnalyzer.Database;
public static class DatabaseSchemaInspector
{
    public static List<string> GetTables(string connectionString)
    {
        var tables = new List<string>();

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string sql = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME";

        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public static List<string> GetStoredProcedures(string connectionString)
    {
        var procs = new List<string>();

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string sql = @"
            SELECT ROUTINE_NAME
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_NAME";

        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            procs.Add(reader.GetString(0));
        }

        return procs;
    }
}