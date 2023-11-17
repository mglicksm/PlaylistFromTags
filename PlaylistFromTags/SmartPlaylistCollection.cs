using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    public class SmartPlaylistCollection 
    {
        [XmlElement("MusicRootFolder")]
        public string MusicRootFolder { get; set; }

        [XmlArray("SmartPlaylistCollection")]

        [XmlArrayItem("SmartPlaylist", typeof(SmartPlaylist))]
        public ArrayList  smartPlaylists;
    }
}
