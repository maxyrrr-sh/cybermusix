using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace cm
{
    class User
    {
        const string connectionString = "Data Source=data.db;Version=3;";
        private int id { get; set; }
        private string usrnm { get; set; }
        private string hashedPsswd { get; set; }
        private string playlistName { get; set; }

        public User(string usrnm, string psswd)
        {
            this.usrnm = usrnm;
            this.hashedPsswd = HashPassword(psswd);
            this.id = setId();
            createPlaylist();
            playlistName = usrnm + "_list";
        }

        public string getPlaylistName()
        {
            return playlistName;
        }

        public string getUsername()
        {
            return this.usrnm;
        }

        public void Register()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE username = @Username;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", usrnm);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine("Користувач з таким ім'ям вже існує.");
                        return;
                    }
                }
                query = "INSERT INTO Users (username, password) VALUES (@Username, @Password);";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", usrnm);
                    command.Parameters.AddWithValue("@Password", hashedPsswd);
                    command.ExecuteNonQuery();
                }

                Console.WriteLine("Новий користувач зареєстрований.");
            }
        }

        public bool Login()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE username = @Username AND password = @Password;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", usrnm);
                    command.Parameters.AddWithValue("@Password", hashedPsswd);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine("Успішний вхід.");
                        return true;

                    }
                }

                Console.WriteLine("Невірне ім'я користувача або пароль.");
                return false;
            }
        }
        
        public List<PlaylistSong> GetPlaylistSongs()
        {
            List<PlaylistSong> playlistSongs = new List<PlaylistSong>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string getPlaylistQuery = $@"
                SELECT SongId, Name FROM {playlistName} 
                INNER JOIN songs ON {playlistName}.SongId = songs.Id;";
                using (SQLiteCommand getPlaylistCommand = new SQLiteCommand(getPlaylistQuery, connection))
                {
                    try
                    {
                        SQLiteDataReader reader = getPlaylistCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            int songId = reader.GetInt32(0);
                            string songName = reader.GetString(1);
                            playlistSongs.Add(new PlaylistSong { Id = songId, Name = songName });
                        }
                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            return playlistSongs;
        }

        void createPlaylist()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                CREATE TABLE IF NOT EXISTS @name (
                    Id INTEGER PRIMARY KEY,
                );";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", playlistName);
            }
        }  

        public void AddSongToPlaylist(int songId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Перевірка наявності плейлиста користувача
                if (this.usrnm != null)
                {
                    string playlistName = usrnm + "_list";

                    // Пошук або створення таблиці плейлиста
                    string createPlaylistQuery = $@"
                CREATE TABLE IF NOT EXISTS {playlistName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SongId INTEGER
                );";
                    using (SQLiteCommand createPlaylistCommand = new SQLiteCommand(createPlaylistQuery, connection))
                    {
                        createPlaylistCommand.ExecuteNonQuery();
                    }

                    // Додавання пісні до плейлиста
                    string addSongQuery = $@"INSERT INTO {playlistName} (SongId) VALUES (@SongId);";
                    using (SQLiteCommand addSongCommand = new SQLiteCommand(addSongQuery, connection))
                    {
                        addSongCommand.Parameters.AddWithValue("@SongId", songId);
                        addSongCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Пісня з id {songId} додана до плейлиста користувача з id.");
                }
                else
                {
                    Console.WriteLine($"Користувача з id не знайдено.");
                }
            }
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private int setId()
        {
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
}
class PlaylistSong
{
    public int Id { get; set; }
    public string Name { get; set; }
}