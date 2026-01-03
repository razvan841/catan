using System;
using Microsoft.Data.Sqlite;
using BCrypt.Net;

namespace Catan.Server;

public static class Db
{
    private const string DbFile = "catan.db";

    static Db() => Initialize();

    public static void Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        // PLAYER TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Player (
                Id TEXT PRIMARY KEY,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                Elo INTEGER NOT NULL DEFAULT 1000
            );";
        cmd.ExecuteNonQuery();

        // GAME TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Game (
                Id TEXT PRIMARY KEY,
                Player1Id TEXT,
                Player2Id TEXT,
                Player3Id TEXT,
                Player4Id TEXT,
                WinnerId TEXT,
                FOREIGN KEY(Player1Id) REFERENCES Player(Id),
                FOREIGN KEY(Player2Id) REFERENCES Player(Id),
                FOREIGN KEY(Player3Id) REFERENCES Player(Id),
                FOREIGN KEY(Player4Id) REFERENCES Player(Id),
                FOREIGN KEY(WinnerId) REFERENCES Player(Id)
            );";
        cmd.ExecuteNonQuery();

        // FRIEND TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Friend (
                Player1Id TEXT NOT NULL,
                Player2Id TEXT NOT NULL,
                PRIMARY KEY(Player1Id, Player2Id),
                FOREIGN KEY(Player1Id) REFERENCES Player(Id),
                FOREIGN KEY(Player2Id) REFERENCES Player(Id)
            );";
        cmd.ExecuteNonQuery();
    }

    // ========================
    // PLAYER METHODS
    // ========================

    public static bool UsernameExists(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT COUNT(*) FROM Player WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        return (long)cmd.ExecuteScalar()! > 0;
    }

    public static string AddUser(string username, string password)
    {
        var userId = Guid.NewGuid().ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Player (Id, Username, PasswordHash, Elo)
            VALUES ($id, $username, $hash, 1000);";

        cmd.Parameters.AddWithValue("$id", userId);
        cmd.Parameters.AddWithValue("$username", username);
        cmd.Parameters.AddWithValue("$hash", hash);

        cmd.ExecuteNonQuery();

        return userId;
    }

    public static string? GetUserId(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id FROM Player WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        return result?.ToString(); // Returns null if user does not exist
    }

    public static bool ValidateUser(string username, string password)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT PasswordHash FROM Player WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        if (result == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, (string)result);
    }

    public static int? GetPlayerElo(string playerId)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT Elo FROM Player WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", playerId);

        var result = cmd.ExecuteScalar();
        if (result == null) return null;
        return Convert.ToInt32(result);
    }

    // ========================
    // GAME METHODS
    // ========================

    public static string AddGame(string? player1Id, string? player2Id, string? player3Id, string? player4Id)
    {
        var gameId = Guid.NewGuid().ToString();

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Game (Id, Player1Id, Player2Id, Player3Id, Player4Id)
            VALUES ($id, $p1, $p2, $p3, $p4);";

        cmd.Parameters.AddWithValue("$id", gameId);
        cmd.Parameters.AddWithValue("$p1", string.IsNullOrEmpty(player1Id) ? (object)DBNull.Value : player1Id);
        cmd.Parameters.AddWithValue("$p2", string.IsNullOrEmpty(player2Id) ? (object)DBNull.Value : player2Id);
        cmd.Parameters.AddWithValue("$p3", string.IsNullOrEmpty(player3Id) ? (object)DBNull.Value : player3Id);
        cmd.Parameters.AddWithValue("$p4", string.IsNullOrEmpty(player4Id) ? (object)DBNull.Value : player4Id);

        cmd.ExecuteNonQuery();
        return gameId;
    }

    public static void SetGameWinner(string gameId, string winnerId)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "UPDATE Game SET WinnerId = $winner WHERE Id = $gameId";
        cmd.Parameters.AddWithValue("$winner", winnerId);
        cmd.Parameters.AddWithValue("$gameId", gameId);

        cmd.ExecuteNonQuery();
    }

    // ========================
    // FRIEND
    // ========================

    public static bool AddFriendship(string player1Id, string player2Id)
    {
        if (string.IsNullOrEmpty(player1Id) || string.IsNullOrEmpty(player2Id))
            return false;

        // prevent duplicate friendship
        if (FriendshipExists(player1Id, player2Id))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Friend (Player1Id, Player2Id)
            VALUES ($p1, $p2);";

        cmd.Parameters.AddWithValue("$p1", player1Id);
        cmd.Parameters.AddWithValue("$p2", player2Id);

        cmd.ExecuteNonQuery();
        return true;
    }

    public static bool FriendshipExists(string player1Id, string player2Id)
    {
        if (string.IsNullOrEmpty(player1Id) || string.IsNullOrEmpty(player2Id))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        // Check for either direction
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Friend 
            WHERE (Player1Id = $p1 AND Player2Id = $p2)
            OR (Player1Id = $p2 AND Player2Id = $p1);";

        cmd.Parameters.AddWithValue("$p1", player1Id);
        cmd.Parameters.AddWithValue("$p2", player2Id);

        long count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    public static List<string> GetFriends(string playerId)
    {
        var friends = new List<string>();
        if (string.IsNullOrEmpty(playerId)) return friends;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT CASE 
                    WHEN Player1Id = $id THEN Player2Id
                    ELSE Player1Id
                END AS FriendId
            FROM Friend
            WHERE Player1Id = $id OR Player2Id = $id;";

        cmd.Parameters.AddWithValue("$id", playerId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            friends.Add(reader.GetString(0));
        }

        return friends;
    }
}
