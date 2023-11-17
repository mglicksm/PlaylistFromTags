using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistFromTags
{
    class SongLibraryEntry : IComparable
    {
        public const string TAGLIB_MIMETYPE_FLAC = "taglib/flac";
        public const string TAGLIB_MIMETYPE_MP3 = "taglib/mp3";

        public string fileName;
        public string fileType;
        public Int32 Duration { get; set; }
        public Int32 StarRating { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public Int32 Year { get; set; }        // Not currently used
        public string FirstAlbumArtist { get; set; }
        public string[] AlbumArtists { get; set; }
        public string FirstPerformer { get; set; }
        public string[] Performers { get; set; }
        public string JoinedPerformers { get; set; }

        // TODO: May want a better comparison for library entries
        virtual public int CompareTo(object obj)
        {
            if (obj is SongLibraryEntry)
            {
                var compareObj = (SongLibraryEntry)obj;
                return this.Duration.CompareTo(compareObj.Duration);
            }
            else
            {
                throw new ArgumentException("Object is not a SongLibraryEntry");
            }
        }

        // FLAC should use multiple entries for 'ARTIST' in mp3tag for perfomers
        // MP3 should use ' / ' between artists
        public SongLibraryEntry(TagLib.File fileTag)
        {
            TagLib.Tag tagXiph;
            TagLib.Tag tagId3v2;

            this.fileName = fileTag.Name;
            fileType = fileTag.MimeType;

            if (fileType == TAGLIB_MIMETYPE_FLAC)
            {
                tagXiph = fileTag.GetTag(TagLib.TagTypes.Xiph);
                Album = tagXiph.Album;
                StarRating = getStarRating(fileType, tagXiph, null);
                FirstAlbumArtist = tagXiph.FirstAlbumArtist;
                FirstPerformer = tagXiph.FirstPerformer;
                JoinedPerformers = tagXiph.JoinedPerformers;

                Performers = (string[])tagXiph.Performers.Clone();
                AlbumArtists = (string[])tagXiph.AlbumArtists.Clone();
                Title = tagXiph.Title;
                Year = Convert.ToInt32(tagXiph.Year);
            }

            else if (fileType == TAGLIB_MIMETYPE_MP3)
            {
                tagId3v2 = fileTag.GetTag(TagLib.TagTypes.Id3v2);
                Album = tagId3v2.Album;
                StarRating = getStarRating(fileType, null, tagId3v2);
                FirstAlbumArtist = tagId3v2.FirstAlbumArtist;
                FirstPerformer = tagId3v2.FirstPerformer;
                JoinedPerformers = tagId3v2.JoinedPerformers;

                Performers = (string[])tagId3v2.Performers.Clone();
                AlbumArtists = (string[])tagId3v2.AlbumArtists.Clone();
                Title = tagId3v2.Title;
                Year = Convert.ToInt32(tagId3v2.Year);
            }
            else
            {
                fileType = null;
                Album = String.Empty;
            }

            Duration = Convert.ToInt32(fileTag.Properties.Duration.TotalSeconds);
        }

        private Int32 getStarRating(string fileType, TagLib.Tag tagXiph, TagLib.Tag tagId3v2)
        {
            Int32 starRating = 0;

            try
            {
                if (fileType == TAGLIB_MIMETYPE_FLAC)
                {
                    Int32 rating = 0;

                    var custom = (TagLib.Ogg.XiphComment)tagXiph;

                    // The RATING field is usually 80 (or higher) for my 5 rating in MP3TAG
                    string[] ratingsField = custom.GetField("RATING");
                    string[] ratingsMMField = custom.GetField("RATING MM");

                    if (ratingsMMField.Length > 0)
                    {
                        rating = Convert.ToInt32(ratingsMMField[0]);
                    }
                    else if (ratingsField.Length > 0)
                    {
                        // Changed above to 'else if' because sometimes we have both and I want to prefer
                        //  the ratingsMMField
                        rating = Convert.ToInt32(ratingsField[0]);
                        if (rating == 80)
                            rating = 5;

                        if (rating != 80 && rating > 5)
                        {
                            // logFileInfo += "Unusual Rating";
                            rating = 5;
                        }
                    }

                    if (rating <= 5 && rating > 4)
                        starRating = 5;
                    else if (rating <= 4 && rating > 3)
                        starRating = 4;
                    else if (rating <= 3 && rating > 2)
                        starRating = 3;
                    else if (rating <= 2 && rating > 1)
                        starRating = 2;
                    else if (rating <= 1 && rating > 0)
                        starRating = 1;
                    else
                        starRating = 0;
                }
                else if (fileType == TAGLIB_MIMETYPE_MP3)
                {
                    TagLib.Id3v2.PopularimeterFrame tagInfo = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tagId3v2, "no@email", false);
                    byte rating = 0;
                    if (tagInfo != null)
                    {
                        rating = tagInfo.Rating;
                    }

                    if (rating <= 255 && rating > 223)
                        starRating = 5;
                    else if (rating <= 223 && rating > 159)
                        starRating = 4;
                    else if (rating <= 159 && rating > 95)
                        starRating = 3;
                    else if (rating <= 95 && rating > 31)
                        starRating = 2;
                    else if (rating <= 31 && rating > 0)
                        starRating = 1;
                    else
                        starRating = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return starRating;
        }
    }
}
