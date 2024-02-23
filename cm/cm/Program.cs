using System.Data.SQLite;
using System.Security.Cryptography.X509Certificates;

const string connectionString = "Data Source=data.db;Version=3;";

List<string> Musics = new List<string>();

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

while (true)
{
    string input = Console.ReadLine();
    string[] _args = input.Split(' ');
    if (_args[0] == "+a")
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();


            FillSongsTable(connection, _args[1]);
        }
    }
    else if (_args[0] == "-q")
    {
        return;
    }
    else if (_args[0] == "-d")
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            int songId = Convert.ToInt32(_args[1]);
            DeleteSongById(connection, songId);
        }
    }
    else if (_args[0] == "-l") 
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            ShowAllSongs(connection);
        }
    }
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
static void DeleteSongById(SQLiteConnection connection, int songId)
{
    string deleteQuery = "DELETE FROM Songs WHERE Id = @SongId;";

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
static void ShowAllSongs(SQLiteConnection connection)
{
    string query = "SELECT Id, Artist, Name FROM Songs;";

    SQLiteCommand command = new SQLiteCommand(query, connection);
    using (SQLiteDataReader reader = command.ExecuteReader())
    {
        Console.WriteLine("ID | Artist | Name");
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader["Id"]);
            string artist = reader["Artist"].ToString();
            string name = reader["Name"].ToString();

            Console.WriteLine($"{id} | {artist} | {name}");
        }
    }
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

class User
{
    private int id { get; set; }
    private string usrnm { get; set; }
    private string psswd { get; set; }

    public User(string usrnm, string psswd)
    {
        this.usrnm = usrnm;
        this.psswd = psswd;
        this.id = setId();
    }

    public static int setId()
    {
        string connectionString = "Data Source=data.db;Version=3;";
        int lastid;
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT COUNT(*) FROM Users;";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            lastid = Convert.ToInt32(command.ExecuteScalar());
        }
        return lastid + 1;
    }
}
