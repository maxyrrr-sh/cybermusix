using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SQLite;
using System.Security.Cryptography.X509Certificates;
using cm;

const string connectionString = "Data Source=data.db;Version=3;";
User user = null;
string session = null;

using (SQLiteConnection connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Songs (
                    Id INTEGER PRIMARY KEY,
                    Artist TEXT,
                    Name TEXT,
                    FileId INTEGER
                );
            ";

    SQLiteCommand createTablesCommand = new SQLiteCommand(createTablesQuery, connection);
    createTablesCommand.ExecuteNonQuery();
}

Console.WriteLine("Please, login or regiser (-l/-r) ");
string input = Console.ReadLine();
string[] _args = input.Split(' ');

if (_args[0] == "-r")
{
    user = new User(_args[1], _args[2]);
    user.Register();
    user.Login();
    session = SessionManager.GenerateToken(_args[1]);
 
}
 else if (_args[0] == "-l")
{
    user = new User(_args[1], _args[2]);
    if (user.Login())
        session = SessionManager.GenerateToken(_args[1]);
}


while (SessionManager.ValidateToken(session, user.getUsername()))
{
    input = Console.ReadLine();
    _args = input.Split(' ');
    if (_args[0] == "+a")
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();


            FillSongsTable(connection, _args[1]);
        }
    }     //add global 
    else if (_args[0] == "-a")
    {
        user.AddSongToPlaylist(Convert.ToInt32(_args[1]));
    }//add personal
    else if (_args[0] == "-q")
    {
        return;
    }//quit
    else if (_args[0] == "-d")
    {
        deleteSong(Convert.ToInt32(_args[1]), user.getPlaylistName());
    }//delete personal
    else if (_args[0] == "+d")
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            int songId = Convert.ToInt32(_args[1]);
            deleteSong(songId, "Songs");
        }
    }//delete global
    else if (_args[0] == "+s")
    {
        var list = GetAllSongs();
        foreach (var song in list)
        {
            Console.WriteLine($" {song.Id}|{song.Name}");
        }
    }//show global
    else if (_args[0] == "-s")
    {
        var list = user.GetPlaylistSongs();
        foreach (var song in list)
        {
            Console.WriteLine($" {song.Id}|{song.Name}");
        }
    }//show personal
}

static void FillSongsTable(SQLiteConnection connection, string directory)
{
    string[] musicFiles = Directory.GetFiles(directory, "*.m4a", SearchOption.AllDirectories);

    foreach (string filePath in musicFiles)
    {
        FileInfo fileInfo = new FileInfo(filePath);

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string artist = "Unknown";
        string insertQuery = $"INSERT INTO Songs (Artist, Name, FileId) VALUES ('{artist}', '{fileName}', @FileId);";

        SQLiteCommand command = new SQLiteCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@FileId", ConvertToByteArray(filePath));

        command.ExecuteNonQuery();
    }
}

static void deleteSong(int songId, string playlistName)
{
    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
    {
        connection.Open();
        string deleteQuery = $"DELETE FROM {playlistName} WHERE Id = @SongId;";

        SQLiteCommand command = new SQLiteCommand(deleteQuery, connection);
        command.Parameters.AddWithValue("@SongId", songId);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            Console.WriteLine($"Пісню з Id = {songId} видалено успішно.");
        }
        else
        {
            Console.WriteLine($"Пісні з Id = {songId} не знайдено.");
        }
    }
}

static List<PlaylistSong> GetAllSongs()
{
    List<PlaylistSong> allSongs = new List<PlaylistSong>();

    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
    {
        connection.Open();

        string query = "SELECT Id, Name FROM Songs;";
        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int songId = reader.GetInt32(0);
                string songName = reader.GetString(1);
                allSongs.Add(new PlaylistSong { Id = songId, Name = songName });
            }
        }
    }

    return allSongs;
}

static byte[] ConvertToByteArray(string filePath)
{
    byte[] fileBytes;
    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    {
        using (BinaryReader br = new BinaryReader(fs))
        {
            fileBytes = br.ReadBytes((int)fs.Length);
        }
    }
    return fileBytes;
}