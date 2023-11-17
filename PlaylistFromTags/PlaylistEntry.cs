using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    class PlaylistEntry : IComparable
    {
        public int position;
        public string artist;
        public string title;
        public int duration;
        public string file;
        public string GetExtInf()
        {
            return String.Format("#EXTINF:{0}, {1} - {2}", duration, artist, title);
        }

        virtual public int CompareTo(object obj)
        {
            if (obj is PlaylistEntry)
            {
                var compareObj = (PlaylistEntry)obj;
                return this.position.CompareTo(compareObj.position);
            }
            else
            {
                throw new ArgumentException("Object is not a MyObject ");
            }
        }
    }

}
