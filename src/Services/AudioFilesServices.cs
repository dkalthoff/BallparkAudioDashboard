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
            return GetSongs(GetFolderPath("SongsPath"));
        }

        public IEnumerable<Song> GetTraditionSongs()
        {
            return GetSongs(GetFolderPath("TraditionSongsPath"));
        }

        public IEnumerable<Song> GetOrganSongs()
        {
            return GetSongs(GetFolderPath("FullOrganAndMiscPath"));
        }

        public IEnumerable<Song> GetSoundClips()
        {
            return GetSongs(GetFolderPath("SoundClipsPath"));
        }

        public IEnumerable<string> GetWalkUpTeamFolderNames()
        {
            List<string> teamFolderNames = new List<string>();
            string walkUpSongsFolderPath = GetFolderPath("WalkUpSongsPath");
            if (Directory.Exists(walkUpSongsFolderPath))
            {
                teamFolderNames = Directory.GetDirectories(walkUpSongsFolderPath).OrderBy(f => f).Select(Path.GetFileName).ToList();
            }

            return teamFolderNames;
        }

        public IEnumerable<Song> GetWalkUpSongs(string folderName)
        {
            return GetSongs(Path.Combine(GetFolderPath("WalkUpSongsPath"), folderName));
        }

        private IEnumerable<Song> GetSongs(string folderPath)
        {
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

        public IEnumerable<Song> GetAllSongs()
        {
            IEnumerable<Song> allSongs = GetFullLengthSongs().Concat(GetTraditionSongs().Concat(GetOrganSongs().Concat(GetSoundClips())));

            foreach (string teamFolderName in GetWalkUpTeamFolderNames())
            {
                allSongs = allSongs.Concat(GetWalkUpSongs(teamFolderName));
            }

            return allSongs;
        }

        private string GetFolderPath(string configurationKey)
        {
            if (ConfigurationManager.AppSettings.Get(configurationKey) == null) throw new Exception(string.Format("Configuration missing \"{0}\" in AppSettings. Add to App.config", configurationKey));
            return ConfigurationManager.AppSettings.Get(configurationKey);
        }

        public bool isSongAMatch(string songTitle, string searchTerm)
        {
            bool isMatch;
            songTitle = songTitle.ToLower();
            searchTerm = searchTerm.ToLower();

            if (searchTerm.Length == 1)
            {
                isMatch = songTitle.StartsWith(searchTerm) || songTitle.Contains(string.Format(" {0} ", searchTerm)) || songTitle.Contains(string.Format(" {0}", searchTerm));
            }
            else
            {
                isMatch = songTitle.Contains(searchTerm);
            }

            return isMatch;
        }
    }
}
