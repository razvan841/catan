using Microsoft.Data.Sqlite;
using BCrypt.Net;

namespace Catan.Server;

public static class Db
{
    private const string DbFile = "catan.db";

    static Db()
    {
        Initialize();
    }

    public static void Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL
        );";
        cmd.ExecuteNonQuery();
    }

    public static bool UsernameExists(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        long count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    public static void AddUser(string username, string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO users (username, password) VALUES ($username, $password)";
        cmd.Parameters.AddWithValue("$username", username);
        cmd.Parameters.AddWithValue("$password", hash);
        cmd.ExecuteNonQuery();
    }

    public static bool ValidateUser(string username, string password)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT password FROM users WHERE username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        if (result == null) return false;

        var hashFromDb = (string)result;

        // Check the provided password against the hash
        return BCrypt.Net.BCrypt.Verify(password, hashFromDb);
    }
}
