using BallparkAudioDashboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BallparkAudioDashboard.Services
{
    public class AudioFilesServices
    {
        public IEnumerable<Song> GetFullLengthSongs()
        {
            return GetSongs("SongsPath");
        }

        public IEnumerable<Song> GetTraditionSongs()
        {
            return GetSongs("TraditionSongsPath");
        }

        public IEnumerable<Song> GetOrganSongs()
        {
            return GetSongs("FullOrganAndMiscPath");
        }

        public IEnumerable<Song> GetSoundClips()
        {
            return GetSongs("SoundClipsPath");
        }

        public IEnumerable<Song> GetWalkUpSongs()
        {
            return GetSongs("WalkUpSongsPath");
        }

        private IEnumerable<Song> GetSongs(string configurationKey)
        {
            string folderPath = GetFolderPath(configurationKey);

            if (Directory.Exists(folderPath))
            {
                return new DirectoryInfo(folderPath).GetFiles().Select(file => new Song
                {
                    Title = file.Name.Replace(file.Extension, string.Empty),
                    FullPath = file.FullName
                }).OrderBy(s => s.Title);
            }

            return new List<Song>();
        }

        private string GetFolderPath(string configurationKey)
        {
            if (ConfigurationManager.AppSettings.Get(configurationKey) == null) throw new Exception(string.Format("Configuration missing \"{0}\" in AppSettings. Add to App.config", configurationKey));
            return ConfigurationManager.AppSettings.Get(configurationKey);
        }
    }
}
