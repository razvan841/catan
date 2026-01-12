using Microsoft.Data.Sqlite;
using Catan.Shared.Models;
using Catan.Shared.Networking.Dtos.Server;
using Catan.Server.Sessions;

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
            Date INTEGER NOT NULL,
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

        // FRIEND REQUEST TABLE
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS FriendRequest (
            fromUserId TEXT NOT NULL,
            toUserId TEXT NOT NULL,
            PRIMARY KEY(fromUserId, toUserId),
            FOREIGN KEY(fromUserId) REFERENCES User(Id),
            FOREIGN KEY(toUserId) REFERENCES User(Id)
        );";
        cmd.ExecuteNonQuery();

        // BLOCKED TABLE
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Blocked (
            blockerId TEXT NOT NULL,
            blockedId TEXT NOT NULL,
            PRIMARY KEY(blockerId, blockedId),
            FOREIGN KEY(blockerId) REFERENCES User(Id),
            FOREIGN KEY(blockedId) REFERENCES User(Id)
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
            string username = $"test-{i}";
            string password = "password";
            string userId = Guid.NewGuid().ToString();
            int elo = Rng.Next(800, 1500);

            cmd.CommandText = @"
                INSERT INTO User (Id, Username, Password, Elo)
                VALUES ($id, $username, $password, $elo)";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$id", userId);
            cmd.Parameters.AddWithValue("$username", username);
            cmd.Parameters.AddWithValue("$123", BCrypt.Net.BCrypt.HashPassword(password));
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

        int gameCount = 30;

        for (int g = 0; g < gameCount; g++)
        {
            int playerCount = Rng.Next(2, 5);
            var players = userIds
                .OrderBy(_ => Rng.Next())
                .Take(playerCount)
                .ToArray();

            string? p1 = players.ElementAtOrDefault(0);
            string? p2 = players.ElementAtOrDefault(1);
            string? p3 = players.ElementAtOrDefault(2);
            string? p4 = players.ElementAtOrDefault(3);

            string winner = players[Rng.Next(players.Length)];
            long date = DateTimeOffset.UtcNow
                .AddDays(-Rng.Next(0, 60))
                .ToUnixTimeSeconds();

            cmd.CommandText = @"
                INSERT INTO Game
                    (Id, user1Id, user2Id, user3Id, user4Id, WinnerId, Date)
                VALUES
                    ($id, $p1, $p2, $p3, $p4, $winner, $date);";

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("$p1", (object?)p1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$p2", (object?)p2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$p3", (object?)p3 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$p4", (object?)p4 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$winner", winner);
            cmd.Parameters.AddWithValue("$date", date);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("Test users, friendships, and games created.");
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
    public static string? GetUsernameById(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT Username FROM User WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", userId);

        var result = cmd.ExecuteScalar();
        return result?.ToString();
    }

    // ========================
    // GAME METHODS
    // ========================

    public static string AddGame(string? user1Id, string? user2Id, string? user3Id, string? user4Id)
    {
        var gameId = Guid.NewGuid().ToString();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Game (Id, user1Id, user2Id, user3Id, user4Id, Date)
            VALUES ($id, $p1, $p2, $p3, $p4, $date);";

        cmd.Parameters.AddWithValue("$id", gameId);
        cmd.Parameters.AddWithValue("$p1", (object?)user1Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$p2", (object?)user2Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$p3", (object?)user3Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$p4", (object?)user4Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$date", timestamp);

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

    public static MatchHistoryResponseEntryDto[] GetMatchHistory(string userId, int limit = 10)
    {
        var matches = new List<MatchHistoryResponseEntryDto>();

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT user1Id, user2Id, user3Id, user4Id, WinnerId
            FROM Game
            WHERE user1Id = $id OR user2Id = $id OR user3Id = $id OR user4Id = $id
            ORDER BY Date DESC
            LIMIT $limit;";
        cmd.Parameters.AddWithValue("$id", userId);
        cmd.Parameters.AddWithValue("$limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var userEntries = new List<LeaderboardEntryDto>();

            for (int i = 0; i < 4; i++)
            {
                if (reader.IsDBNull(i)) continue;
                var uid = reader.GetString(i);
                var username = GetUsernameById(uid);
                var elo = GetUserElo(uid);

                if (username != null && elo != null)
                {
                    userEntries.Add(new LeaderboardEntryDto
                    {
                        Username = username,
                        Elo = elo.Value
                    });
                }
            }

            string winnerName = "";
            if (!reader.IsDBNull(4))
            {
                winnerName = GetUsernameById(reader.GetString(4)) ?? "";
            }

            matches.Add(new MatchHistoryResponseEntryDto
            {
                UserEntries = userEntries.ToArray(),
                Winner = winnerName
            });
        }

        return matches.ToArray();
    }


    // ========================
    // FRIEND
    // ========================

    public static bool AddFriendship(string user1Id, string user2Id)
    {
        if (string.IsNullOrEmpty(user1Id) || string.IsNullOrEmpty(user2Id))
            return false;

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
            OR (user1Id = $p2 AND user2Id = $p1);";

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

    public static FriendListResponseEntryDto[] GetFriendsWithStatusFiltered( string userId, Func<string, ClientSession?> sessionLookup)
    {
        var entries = new List<FriendListResponseEntryDto>();

        foreach (var fid in GetFriends(userId))
        {
            if (BlockExists(userId, fid) || BlockExists(fid, userId))
                continue;

            var username = GetUsernameById(fid);
            if (username == null)
                continue;

            var friendSession = sessionLookup(fid);

            entries.Add(new FriendListResponseEntryDto
            {
                Username = username,
                Online = friendSession != null
            });
        }

        return entries.ToArray();
    }


    // ========================
    // FRIEND REQUESTS
    // ========================

    public static bool AddFriendRequest(string fromUserId, string toUserId)
    {
        if (FriendshipExists(fromUserId, toUserId))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT OR IGNORE INTO FriendRequest (fromUserId, toUserId)
            VALUES ($from, $to);";

        cmd.Parameters.AddWithValue("$from", fromUserId);
        cmd.Parameters.AddWithValue("$to", toUserId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool RemoveFriendRequest(string fromUserId, string toUserId)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            DELETE FROM FriendRequest
            WHERE fromUserId = $from AND toUserId = $to;";

        cmd.Parameters.AddWithValue("$from", fromUserId);
        cmd.Parameters.AddWithValue("$to", toUserId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool FriendRequestExists(string fromUserId, string toUserId)
    {
        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT COUNT(*) FROM FriendRequest
            WHERE fromUserId = $from AND toUserId = $to;";

        cmd.Parameters.AddWithValue("$from", fromUserId);
        cmd.Parameters.AddWithValue("$to", toUserId);

        return (long)cmd.ExecuteScalar()! > 0;
    }
    public static bool RemoveFriendship(string user1Id, string user2Id)
    {
        if (string.IsNullOrEmpty(user1Id) || string.IsNullOrEmpty(user2Id))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            DELETE FROM Friend
            WHERE (user1Id = $u1 AND user2Id = $u2)
            OR (user1Id = $u2 AND user2Id = $u1);";

        cmd.Parameters.AddWithValue("$u1", user1Id);
        cmd.Parameters.AddWithValue("$u2", user2Id);

        return cmd.ExecuteNonQuery() > 0;
    }


    // ========================
    // BLOCKED USERS
    // ========================

    
    public static bool AddBlock(string blockerId, string blockedId)
    {
        if (string.IsNullOrEmpty(blockerId) || string.IsNullOrEmpty(blockedId))
            return false;

        if (BlockExists(blockerId, blockedId))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Blocked (blockerId, blockedId)
            VALUES ($blocker, $blocked);";
        cmd.Parameters.AddWithValue("$blocker", blockerId);
        cmd.Parameters.AddWithValue("$blocked", blockedId);
        cmd.ExecuteNonQuery();
        RemoveFriendship(blockerId, blockedId);

        return true;
    }

    public static bool BlockExists(string blockerId, string blockedId)
    {
        if (string.IsNullOrEmpty(blockerId) || string.IsNullOrEmpty(blockedId))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT COUNT(*) FROM Blocked
            WHERE blockerId = $blocker AND blockedId = $blocked;";
        cmd.Parameters.AddWithValue("$blocker", blockerId);
        cmd.Parameters.AddWithValue("$blocked", blockedId);

        long count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    public static bool RemoveBlock(string blockerId, string blockedId)
    {
        if (string.IsNullOrEmpty(blockerId) || string.IsNullOrEmpty(blockedId))
            return false;

        using var conn = new SqliteConnection($"Data Source={DbFile}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            DELETE FROM Blocked
            WHERE blockerId = $blocker AND blockedId = $blocked;";
        cmd.Parameters.AddWithValue("$blocker", blockerId);
        cmd.Parameters.AddWithValue("$blocked", blockedId);

        return cmd.ExecuteNonQuery() > 0;
    }

}
