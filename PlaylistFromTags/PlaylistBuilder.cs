using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    class PlaylistBuilder
    {
        public const string LOG_FILE_NAME = "PlaylistFromTags.log";

        public static void CreatePlaylist()
        {
            
        }
        
        static private bool isRatingOutOfRange(SmartPlaylist spl, int rating)
        {
            bool skipFile = false;
            if (spl.Ratings[0] == true || spl.Ratings[1] == true || spl.Ratings[2] == true || spl.Ratings[3] == true || spl.Ratings[4] == true)
            {
                skipFile = true;
                bool isOneStar = false;
                bool isTwoStars = false;
                bool isThreeStars = false;
                bool isFourStars = false;
                bool isFiveStars = false;
                bool isZeroStar = false;

                if (rating <= 5 && rating > 4)
                    isFiveStars = true;
                else if (rating <= 4 && rating > 3)
                    isFourStars = true;
                else if (rating <= 3 && rating > 2)
                    isThreeStars = true;
                else if (rating <= 2 && rating > 1)
                    isTwoStars = true;
                else if (rating <= 1 && rating > 0)
                    isOneStar = true;
                else
                    isZeroStar = true;

                if (isZeroStar)
                    // We checked stars so we don't want any zero star files
                    return true;

                if (spl.Ratings[0] == true && isOneStar)
                    skipFile = false;
                else if (spl.Ratings[1] == true && isTwoStars)
                    skipFile = false;
                else if (spl.Ratings[2] == true && isThreeStars)
                    skipFile = false;
                else if (spl.Ratings[3] == true && isFourStars)
                    skipFile = false;
                else if (spl.Ratings[4] == true && isFiveStars)
                    skipFile = false;
            }

            return skipFile;
        }
        
        static public void CreatePlaylistFromCollection(SmartPlaylistCollection splc)
        {
            StringBuilder logBuilder = new StringBuilder();
            ArrayList builderItems = new ArrayList();

            SongLibrary library = new SongLibrary();
            library.songList = new ArrayList();

            foreach (SmartPlaylist spl in splc.smartPlaylists)
            {
                PlaylistBuilderItem plbi = new PlaylistBuilderItem();
                plbi.playList = new ArrayList();
                plbi.spl = spl;
                builderItems.Add(plbi); 
            }

            string[] files = System.IO.Directory.GetFiles(splc.MusicRootFolder, "*.*", System.IO.SearchOption.AllDirectories);

            // Process each song file
            foreach (string fileName in files)
            {
                string fileType = System.IO.Path.GetExtension(fileName).ToLower();

                if (fileType == ".mp3" || fileType == ".flac")
                {
                    var fileTag = TagLib.File.Create(fileName);

                    // Create the library entry from the tag
                    SongLibraryEntry sle = new SongLibraryEntry(fileTag);
                    library.songList.Add(sle);
                }
            }

            // Process each library entry
            foreach (SongLibraryEntry sle in library.songList)
            {
                logBuilder.Append(String.Format("\"{0}\" | {1} | {2} | {3}\n", sle.fileName, sle.fileType, sle.StarRating, sle.Duration));
                foreach (PlaylistBuilderItem bi in builderItems)
                {
                    ProcessSongFileFromLibrary(bi.spl, splc.MusicRootFolder, bi.playList, sle, logBuilder);
                }
            }

            // Process the built playlists, writing them to M3U files
            foreach (PlaylistBuilderItem bi in builderItems)
            {
                logBuilder.Append("Writing \"" + bi.spl.PlaylistName + "\" | " + bi.playList.Count.ToString() + "\n");
                bi.playList.Sort();     // Sort by position
                bi.WriteM3U();
            }

            File.Delete(splc.MusicRootFolder + "\\" + LOG_FILE_NAME);
            File.AppendAllText(splc.MusicRootFolder + "\\" + LOG_FILE_NAME, logBuilder.ToString());
            logBuilder.Clear();
        }
        
        static private void ProcessSongFileFromLibrary(SmartPlaylist spl, String musicRootFolder, ArrayList playList, SongLibraryEntry sle, StringBuilder logBuilder)
        {
            bool ratingsMatch = false;
            bool artistMatch = false;
            bool albumArtistMatch = false;
            bool albumMatch = false;

            bool hasArtistFilter = false;
            bool hasAlbumArtistFilter = false;
            bool hasAlbumFilter = false;

            String logFileInfo = sle.fileName;

            if (sle.fileType == SongLibraryEntry.TAGLIB_MIMETYPE_MP3 || sle.fileType == SongLibraryEntry.TAGLIB_MIMETYPE_FLAC)
            {
                // Write the file extension to the log
                logFileInfo += " | " + sle.fileType + " | " + sle.StarRating;

                // Sometimes, tag is "RATING WMP" which I cannot find through code. Manually fixed those.
                ratingsMatch = !isRatingOutOfRange(spl, sle.StarRating);

                if (spl.Artists != null && spl.Artists.Length > 0)
                {
                    hasArtistFilter = true;
                    foreach (string artistName in spl.Artists)
                    {
                        // Useful debugging
                        if (artistName.ToUpper().Trim().Equals("LINDA RONDSTAT") && (sle.Album != null && sle.Album.Trim().ToUpper().Equals("TRIO II")))
                            logFileInfo += "Here";

                        if (sle.FirstPerformer != null && artistName.ToUpper().Trim().Equals(sle.FirstPerformer.ToUpper().Trim()))
                        {
                            artistMatch = true;
                            break;
                        }
                        else
                        {
                            // MJG - taglib requires a ' / ' between each artist.
                            for (int i = 0; i < sle.Performers.Length; i++)
                            {
                                if (artistName.ToUpper().Trim().Equals(sle.Performers[i].ToUpper().Trim()))
                                {
                                    artistMatch = true;
                                    break;
                                }
                            }
                            // Try for an album artist match or joined artist match
                            /* // MJG - I don't want to find album artists if looking for Artists.
                            * //  Causes false phil collins matches to his 'plays well with others' stuff
                            * //  TODO: Guessing might want this for Linda Rondstat 'Trio' or something like that
                            * //   where the artist I really want is the Album Artist
                            if (tag.FirstAlbumArtist != null && artistName.ToUpper().Trim().Equals(tag.FirstAlbumArtist.ToUpper().Trim()))
                            {
                                skipFile = false;
                                break;
                            }
                            */

                            // Try for an joined artist match or joined artist match
                            if (sle.JoinedPerformers != null && artistName.ToUpper().Trim().Equals(sle.JoinedPerformers.ToUpper().Trim()))
                            {
                                artistMatch = true;
                                break;
                            }
                        }
                    }
                }

                if ( spl.AlbumArtists != null && spl.AlbumArtists.Length > 0)
                {
                    hasAlbumArtistFilter = true;
                    foreach (string artistName in spl.AlbumArtists)
                    {
                        if (sle.FirstAlbumArtist == null)
                            continue;

                        if (artistName.ToUpper().Trim().Equals(sle.FirstAlbumArtist.ToUpper().Trim()))
                        {
                            albumArtistMatch = true;
                            break;
                        }
                    }
                }

                if  (spl.Albums != null && spl.Albums.Length > 0)
                {
                    hasAlbumFilter = true;
                    foreach (string albumName in spl.Albums)
                    {
                        if (sle.Album == null)
                            continue;

                        if (albumName.ToUpper().Trim().Equals(sle.Album.ToUpper().Trim()))
                        {
                            albumMatch = true;
                            break;
                        }
                    }
                }

                // If not filtering by album, then allow all albums to match
                if (!hasAlbumFilter)
                    albumMatch = true;

                // If not filtering by album artist AND not by artist, allow all artists to match
                if (!hasAlbumArtistFilter && !hasArtistFilter)
                {
                    albumArtistMatch = true;
                    artistMatch = true;
                }

                // Conditions must be met in order to go on this playlist
                if (ratingsMatch && albumMatch && ( albumArtistMatch || artistMatch) )
                {
                    PlaylistEntry item = new PlaylistEntry();

                    if (sle.AlbumArtists.Length > 0)
                        item.artist = sle.AlbumArtists[0];
                    else
                    {
                        if (sle.FirstAlbumArtist != null && sle.FirstAlbumArtist.Trim().Length > 0)
                            item.artist = sle.FirstAlbumArtist;
                        else
                        {
                            if (sle.Performers.Length > 0)
                                item.artist = sle.Performers[0];
                            else
                                item.artist = sle.FirstPerformer;
                        }
                    }

                    item.title = sle.Title;
                    item.duration = sle.Duration;
                    item.file = sle.fileName.Replace(musicRootFolder, "").TrimStart('\\').Replace(@"\", "/");
                    if (!spl.Shuffle)
                        item.position = playList.Count + 1;
                    else
                        item.position = spl.GenerateRandomNumber();

                    playList.Add(item);
                }
            }
            else
            {
                logFileInfo += " | Not a music file";
            }
        }
    }
}
