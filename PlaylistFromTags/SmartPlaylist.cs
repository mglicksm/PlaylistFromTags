using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    public class SmartPlaylist
    {
        private readonly Random random = new Random();

        public string Id { get; set; }
        public string PlaylistName { get; set; }
        public string [] Artists { get; set;  }
        public string [] Genres { get; set;  }
        public string [] AlbumArtists { get; set; }
        public string[] Albums { get; set; }
        public bool [] Ratings { get; set;  }
        public bool Shuffle { get; set; }

        public int GenerateRandomNumber()
        {
            return this.random.Next(8000);
        }
    }
}
