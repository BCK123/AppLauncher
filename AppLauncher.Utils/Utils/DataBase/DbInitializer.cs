using AppLauncher.Utils.DataBase;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AppLauncher.Utils.Utils.DataBase
{
  

    public static class DbInitializer
    {
        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AppPaths.DbPath)!);

            using var conn = new SqliteConnection(
                $"Data Source={AppPaths.DbPath}");

            conn.Open();

            CreateTables(conn);
            SeedData(conn);
        }

        private static void CreateTables(SqliteConnection conn)
        {
            conn.Execute("""
        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL UNIQUE
        );

        CREATE TABLE IF NOT EXISTS Shortcuts (
            Id TEXT PRIMARY KEY,
            DisplayName TEXT NOT NULL,
            TargetPath TEXT NOT NULL,
            Category TEXT NOT NULL
        );
        """);
        }

        private static void SeedData(SqliteConnection conn)
        {
            var count = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Categories");

            if (count == 0)
            {
                conn.Execute(
                    "INSERT INTO Categories (Name) VALUES (@name)",
                    new { name = "全部" });
            }
        }
    }

}
