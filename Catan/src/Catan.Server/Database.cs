using Microsoft.Data.Sqlite;
using Catan.Shared.Models;

namespace Catan.Server;

public static class Db
{
    private const string DbFile = "catan.db";
    private static readonly Random Rng = new Random();

    static Db() => Initialize();

    public static void Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        // user TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS User (
                Id TEXT PRIMARY KEY,
                Username TEXT UNIQUE NOT NULL,
                Password TEXT NOT NULL,
                Elo INTEGER NOT NULL DEFAULT 1000
            );";
        cmd.ExecuteNonQuery();

        // GAME TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Game (
                Id TEXT PRIMARY KEY,
                user1Id TEXT,
                user2Id TEXT,
                user3Id TEXT,
                user4Id TEXT,
                WinnerId TEXT,
                FOREIGN KEY(user1Id) REFERENCES User(Id),
                FOREIGN KEY(user2Id) REFERENCES User(Id),
                FOREIGN KEY(user3Id) REFERENCES User(Id),
                FOREIGN KEY(user4Id) REFERENCES User(Id),
                FOREIGN KEY(WinnerId) REFERENCES User(Id)
            );";
        cmd.ExecuteNonQuery();

        // FRIEND TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Friend (
                user1Id TEXT NOT NULL,
                user2Id TEXT NOT NULL,
                PRIMARY KEY(user1Id, user2Id),
                FOREIGN KEY(user1Id) REFERENCES User(Id),
                FOREIGN KEY(user2Id) REFERENCES User(Id)
            );";
        cmd.ExecuteNonQuery();

        // SeedTestData();
    }

    private static void SeedTestData()
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();

        var userIds = new List<string>();

        for (int i = 1; i <= 8; i++)
        {
            string username = $"test-user{i}";
            string password = "password";
            string userId = Guid.NewGuid().ToString();
            int elo = Rng.Next(800, 1500);

            cmd.CommandText = "INSERT INTO User (Id, Username, Password, Elo) VALUES ($id, $username, $password, $elo)";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$id", userId);
            cmd.Parameters.AddWithValue("$username", username);
            cmd.Parameters.AddWithValue("$password", BCrypt.Net.BCrypt.HashPassword(password));
            cmd.Parameters.AddWithValue("$elo", elo);
            cmd.ExecuteNonQuery();

            userIds.Add(userId);
        }

        for (int i = 0; i < userIds.Count; i++)
        {
            for (int j = i + 1; j < userIds.Count; j++)
            {
                if (Rng.NextDouble() < 0.5)
                {
                    cmd.CommandText = "INSERT INTO Friend (user1Id, user2Id) VALUES ($u1, $u2)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$u1", userIds[i]);
                    cmd.Parameters.AddWithValue("$u2", userIds[j]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine("Test users and friendships created.");
    }

    // ========================
    // USER METHODS
    // ========================

    public static bool UsernameExists(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT COUNT(*) FROM User WHERE Username = $username";
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
            INSERT INTO User (Id, Username, Password, Elo)
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
        cmd.CommandText = "SELECT Id FROM User WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        return result?.ToString(); // Returns null if user does not exist
    }

    public static bool ValidateUser(string username, string password)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT Password FROM User WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        if (result == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, (string)result);
    }

    public static int? GetUserElo(string userId)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT Elo FROM User WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", userId);

        var result = cmd.ExecuteScalar();
        if (result == null) return null;
        return Convert.ToInt32(result);
    }

    public static int? GetEloByUsername(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Elo FROM User WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        var result = cmd.ExecuteScalar();
        if (result == null) return null;

        return Convert.ToInt32(result);
    }

    public static (string Username, int Elo)[] GetLeaderboard(int topN)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Username, Elo FROM User ORDER BY Elo DESC LIMIT $topN";
        cmd.Parameters.AddWithValue("$topN", topN);

        var list = new List<(string, int)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string username = reader.GetString(0);
            int elo = reader.GetInt32(1);
            list.Add((username, elo));
        }

        return list.ToArray();
    }

    public static PlayerInfo? GetUserInfoByUsername(string username)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Elo FROM User WHERE Username = $username";
        cmd.Parameters.AddWithValue("$username", username);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        var userId = reader.GetString(0);
        var elo = reader.GetInt32(1);

        var friendsCmd = conn.CreateCommand();
        friendsCmd.CommandText = @"
            SELECT u.Username
            FROM Friend f
            JOIN User u ON (u.Id = f.user2Id)
            WHERE f.user1Id = $userId
            UNION
            SELECT u.Username
            FROM Friend f
            JOIN User u ON (u.Id = f.user1Id)
            WHERE f.user2Id = $userId";
        friendsCmd.Parameters.AddWithValue("$userId", userId);

        var friendsList = new List<string>();
        using var friendsReader = friendsCmd.ExecuteReader();
        while (friendsReader.Read())
        {
            friendsList.Add(friendsReader.GetString(0));
        }

        return new PlayerInfo
        {
            Username = username,
            Elo = elo,
            Friends = friendsList.ToArray()
        };
    }

    // ========================
    // GAME METHODS
    // ========================

    public static string AddGame(string? user1Id, string? user2Id, string? user3Id, string? user4Id)
    {
        var gameId = Guid.NewGuid().ToString();

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Game (Id, user1Id, user2Id, user3Id, user4Id)
            VALUES ($id, $p1, $p2, $p3, $p4);";

        cmd.Parameters.AddWithValue("$id", gameId);
        cmd.Parameters.AddWithValue("$p1", string.IsNullOrEmpty(user1Id) ? (object)DBNull.Value : user1Id);
        cmd.Parameters.AddWithValue("$p2", string.IsNullOrEmpty(user2Id) ? (object)DBNull.Value : user2Id);
        cmd.Parameters.AddWithValue("$p3", string.IsNullOrEmpty(user3Id) ? (object)DBNull.Value : user3Id);
        cmd.Parameters.AddWithValue("$p4", string.IsNullOrEmpty(user4Id) ? (object)DBNull.Value : user4Id);

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

    public static bool AddFriendship(string user1Id, string user2Id)
    {
        if (string.IsNullOrEmpty(user1Id) || string.IsNullOrEmpty(user2Id))
            return false;

        // prevent duplicate friendship
        if (FriendshipExists(user1Id, user2Id))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Friend (user1Id, user2Id)
            VALUES ($p1, $p2);";

        cmd.Parameters.AddWithValue("$p1", user1Id);
        cmd.Parameters.AddWithValue("$p2", user2Id);

        cmd.ExecuteNonQuery();
        return true;
    }

    public static bool FriendshipExists(string user1Id, string user2Id)
    {
        if (string.IsNullOrEmpty(user1Id) || string.IsNullOrEmpty(user2Id))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        // Check for either direction
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Friend 
            WHERE (user1Id = $p1 AND user2Id = $p2)
            OR (userId = $p2 AND user2Id = $p1);";

        cmd.Parameters.AddWithValue("$p1", user1Id);
        cmd.Parameters.AddWithValue("$p2", user2Id);

        long count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    public static List<string> GetFriends(string userId)
    {
        var friends = new List<string>();
        if (string.IsNullOrEmpty(userId)) return friends;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT CASE 
                    WHEN user1Id = $id THEN user2Id
                    ELSE user1Id
                END AS FriendId
            FROM Friend
            WHERE user1Id = $id OR user2Id = $id;";

        cmd.Parameters.AddWithValue("$id", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            friends.Add(reader.GetString(0));
        }

        return friends;
    }


}
