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
    public IActionResult AddMusicFromFolder()
    {
        string folderPath = @"C:\Users\User\Downloads";
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

        return RedirectToAction("Playlist");
    }

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
            if (SessionManager.ValidateToken(HttpContext.Request.Cookies["SessionToken"], cm.User.user.getUsername()))
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
            if (SessionManager.ValidateToken(HttpContext.Request.Cookies["SessionToken"], cm.User.user.getUsername()))
            {
                var model = GetPersonalMusic();
                return View(model);
            }
            else return RedirectToAction("SessionError");
        }
        catch (System.NullReferenceException ex)
        {
            return RedirectToAction("SessionError", "Home");

        }
    }

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
        var personalMusic = new List<Music>();

        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();

            var selectQuery = $"SELECT Songs.* FROM Songs INNER JOIN {cm.User.user.getPlaylistName()} ON Songs.Id = {cm.User.user.getPlaylistName()}.SongId";
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

                    personalMusic.Add(music);
                }
            }
        }

        return personalMusic;
    }

    public IActionResult Delete(int songId)
    {
        var deleteQuery = $"DELETE FROM {cm.User.user.getPlaylistName()} WHERE SongId = @SongId";

        using (var connection = new SQLiteConnection("Data Source=data.db"))
        {
            connection.Open();
            var command = new SQLiteCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@SongId", songId);
            command.ExecuteNonQuery();
        }

        return RedirectToAction("PersonalPlaylist");
    }

    [HttpPost]
    public IActionResult GetSongId(int songId)
    {
        cm.User.user.AddSongToPlaylist(songId);
        return RedirectToAction("Playlist");  
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
