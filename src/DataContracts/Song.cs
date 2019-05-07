using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallparkAudioDashboard.DataContracts
{
    [Serializable]
    public class Song
    {
        public string Title { get; set; }
        public string FullPath { get; set; }
    }
}
