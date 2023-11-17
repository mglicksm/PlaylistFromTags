using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

namespace PlaylistFromTags
{
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [STAThread]
        public static int Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                // TODO: Add your code to run in command line mode
                Console.WriteLine("PlaylistFromTags");

                XmlSerializer ser = new XmlSerializer(typeof(SmartPlaylistCollection));
                FileStream fs = new FileStream(args[0], FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);


                // XmlSerializer ser = new XmlSerializer(typeof(SmartPlaylist));
                // FileStream fs = new FileStream(args[0], FileMode.Open);
                // XmlReader reader = XmlReader.Create(fs);
                Console.WriteLine("Processing " + args[0]);

                SmartPlaylistCollection splc = (SmartPlaylistCollection)ser.Deserialize(reader);

                PlaylistBuilder.CreatePlaylistFromCollection(splc);
                /*
                foreach (SmartPlaylist spl in splc.smartPlaylists)
                {
                    PlaylistBuilder.CreatePlaylist(spl, splc.MusicRootFolder);
                }
                */
                Console.WriteLine("Completed " + args[0]);

                // fs.close():

                // Console.ReadLine();
                // Do work
                return 0;
            }
            else
            {
                FreeConsole();
                var app = new App();
                return app.Run();
            }
        }
    }
}


