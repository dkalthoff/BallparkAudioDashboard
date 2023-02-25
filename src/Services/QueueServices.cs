using BallparkAudioDashboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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

            using (Stream stream = File.OpenWrite(Path.Combine(folderPath, name)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
                serializer.Serialize(stream, songs.ToList());
            }
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
            IEnumerable<Song> songs;

            using (FileStream fs = File.Open(Path.Combine(GetFolderPath(), name), FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
                songs = serializer.Deserialize(fs) as List<Song>;
            }

            return new ObservableCollection<Song>(songs);
        }
    }
}
