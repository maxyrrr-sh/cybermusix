using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using cm;

public class Music
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte[] FileData { get; set; }
}

public class MusicController : Controller
{
    // Метод для додавання музики з папки
    public IActionResult AddMusicFromFolder(string folderPath)
    {
        string[] musicFiles = Directory.GetFiles(folderPath, "*.m4a", SearchOption.AllDirectories);

        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();

            foreach (string filePath in musicFiles)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                byte[] fileData = ConvertToByteArray(filePath);

                string insertQuery = $"INSERT INTO Songs (Name, FileData) VALUES (@Name, @FileData);";

                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", fileName);
                command.Parameters.AddWithValue("@FileData", fileData);

                command.ExecuteNonQuery();
            }
        }

        return RedirectToAction("Index");
    }

    // Метод для видалення музики за ідентифікатором
    public IActionResult DeleteMusic(int id)
    {
        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();

            var deleteQuery = "DELETE FROM Songs WHERE Id = @Id";
            var command = new SQLiteCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Playlist()
    {
        try
        {
            if (SessionManager.ValidateToken(SessionManager.sessionToken, cm.User.user.getUsername()))
            {
                var model = GetAllMusic();
                return View(model);
            } else return RedirectToAction("SessionError");
        } catch (System.NullReferenceException ex)
        {
            return RedirectToAction("SessionError", "Home");

        }
    }

    public IActionResult PersonalPlaylist()
    {
        try
        {
            if (SessionManager.ValidateToken(SessionManager.sessionToken, cm.User.user.getUsername()))
            {
                var model = GetAllMusic();
                return View(model);
            }
            else return RedirectToAction("SessionError");
        }
        catch (System.NullReferenceException ex)
        {
            return RedirectToAction("SessionError", "Home");

        }
    }

    // Метод для зчитування музики з бази даних
    private List<Music> GetAllMusic()
    {
        var songs = new List<Music>();

        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();

            var selectQuery = "SELECT * FROM Songs";
            var command = new SQLiteCommand(selectQuery, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var name = reader.GetString(1);
                    var fileData = (byte[])reader["FileData"];

                    var music = new Music
                    {
                        Id = id,
                        Name = name,
                        FileData = fileData
                    };

                    songs.Add(music);
                }
            }
        }

        return songs;
    }
    private List<Music> GetPersonalMusic()
    {
        var songs = new List<Music>();

        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();

            var selectQuery = $"SELECT * FROM {cm.User.user.getPlaylistName()}";
            var command = new SQLiteCommand(selectQuery, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var name = reader.GetString(1);
                    var fileData = (byte[])reader["FileData"];

                    var music = new Music
                    {
                        Id = id,
                        Name = name,
                        FileData = fileData
                    };

                    songs.Add(music);
                }
            }
        }

        return songs;
    }

    [HttpPost]
    public IActionResult GetSongId(int songId)
    {
        cm.User.user.AddSongToPlaylist(songId);
        return RedirectToAction("Index"); // Припустимо, що метод Index відповідає сторінці зі списком пісень
    }

    private byte[] ConvertToByteArray(string filePath)
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

}
