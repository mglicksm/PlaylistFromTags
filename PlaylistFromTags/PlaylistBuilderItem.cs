using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    public class PlaylistBuilderItem
    {
        public SmartPlaylist spl;
        public ArrayList playList;

        public void WriteM3U()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(spl.PlaylistName))
            {
                file.WriteLine("#EXTM3U");

                foreach (PlaylistEntry entry in playList)
                {
                    file.WriteLine(entry.GetExtInf());
                    file.WriteLine(entry.file);
                }
            }
        }


    }
}
