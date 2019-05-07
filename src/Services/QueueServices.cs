using BallparkAudioDashboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BallparkAudioDashboard.Services
{
    public class QueueServices
    {
        private const string QUEUE_FOLDER_PATH_KEY = "SavedQueuesFolderPath";

        private string GetFolderPath()
        {
            if (ConfigurationManager.AppSettings.Get(QUEUE_FOLDER_PATH_KEY) == null) throw new Exception(string.Format("Configuration missing \"{0}\" in AppSettings. Add to App.config", QUEUE_FOLDER_PATH_KEY));
            return ConfigurationManager.AppSettings.Get(QUEUE_FOLDER_PATH_KEY);
        }

        public void Create(string name, IEnumerable<Song> songs)
        {
            string folderPath = GetFolderPath();

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            Stream stream = File.OpenWrite(Path.Combine(folderPath, name));
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, songs.ToList());
            stream.Flush();
            stream.Close();
            stream.Dispose();
        }

        public IEnumerable<string> Get()
        {
            if (Directory.Exists(GetFolderPath()))
            {
                return new DirectoryInfo(GetFolderPath()).GetFiles().Select(file => file.Name);
            }
            else
            {
                return new List<string>();
            }
        }

        public ObservableCollection<Song> Get(string name)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = File.Open(Path.Combine(GetFolderPath(), name), FileMode.Open);

            IEnumerable<Song> songs = formatter.Deserialize(fs) as List<Song>;
            fs.Flush();
            fs.Close();
            fs.Dispose(); 
            return new ObservableCollection<Song>(songs);
        }
    }
}
