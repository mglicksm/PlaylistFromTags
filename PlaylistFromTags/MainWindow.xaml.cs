using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using TagLib.Id3v2;
using System.Xml;
using System.Xml.Serialization;

namespace PlaylistFromTags
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // private SmartPlaylist currentSmartPlaylist = new SmartPlaylist();
        private SmartPlaylistCollection currentPlaylistCollection = new SmartPlaylistCollection();

        public MainWindow()
        {
            InitializeComponent();
            currentPlaylistCollection.smartPlaylists = new ArrayList();
            currentPlaylistCollection.MusicRootFolder = tbMusicFolder.Text;
        }

        private void btnLaunchBrowse_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = tbMusicFolder.Text;

            using (var fileDialog = new FolderBrowserDialog())
            {
                var result = fileDialog.ShowDialog();
                switch (result)
                {
                    case System.Windows.Forms.DialogResult.OK:
                        var file = fileDialog.SelectedPath;
                        selectedFolder = file;
                        break;
                    case System.Windows.Forms.DialogResult.Cancel:
                    default:
                        selectedFolder = String.Empty;
                        break;

                }

                tbMusicFolder.Text = selectedFolder;
            }
        }

        /// <summary>
        /// Select file
        /// Needs to be a unicode m3u8 file.
        /// </summary>
        /// <returns></returns>
        private string SelectPlaylistPath()
        {
            string selectedApp = String.Empty;
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.InitialDirectory = tbMusicFolder.Text;
            fileDialog.Filter = "Playlist files (*.m3u8)|*.m3u8";
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    selectedApp = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    selectedApp = String.Empty;
                    break;

            }
            return selectedApp;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FindFrames(TagLib.Id3v2.Tag tag)
        {
            PopularimeterFrame frame;
            IEnumerator<TagLib.Id3v2.Frame> enumerator = tag.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TagLib.Id3v2.Frame current = enumerator.Current;
                    frame = current as PopularimeterFrame;
                    if ( frame != null )
                        System.Windows.Forms.MessageBox.Show(frame.User);
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }

        private void CreatePlaylist()
        {
            lblStatusInformation.Text = "Beginning scan...";

            savePlaylistById(tbPlaylistId.Text);

            PlaylistBuilder.CreatePlaylistFromCollection(currentPlaylistCollection);
            lblStatusInformation.Text = "Create Done";
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if ( String.IsNullOrEmpty(tbPlaylistFile.Text))
            {
                lblStatusInformation.Text = "No playlist selected";
                return;
            }
            CreatePlaylist();
        }

        private void btnPlaylistBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button clickButton = (System.Windows.Controls.Button)sender;

                string selectedFile = SelectPlaylistPath();

                tbPlaylistFile.Text = selectedFile;
            }
            catch (Exception ex)
            {
                Log("Exception btnLaunchBrowse_Click: " + ex.Message);
            }

        }

        private void Log(string message)
        {
            // TODO
        }

        private SmartPlaylist GetSmartPlaylistFromUI()
        {
            SmartPlaylist newSmartPlayList = new SmartPlaylist();
            try
            {
                newSmartPlayList.PlaylistName = tbPlaylistFile.Text;
                newSmartPlayList.Id = tbPlaylistId.Text;

                newSmartPlayList.Ratings = new bool[5];
                newSmartPlayList.Ratings[0] = (bool)cb1Star.IsChecked == true;
                newSmartPlayList.Ratings[1] = (bool)cb2Star.IsChecked == true;
                newSmartPlayList.Ratings[2] = (bool)cb3Star.IsChecked == true;
                newSmartPlayList.Ratings[3] = (bool)cb4Star.IsChecked == true;
                newSmartPlayList.Ratings[4] = (bool)cb5Star.IsChecked == true;

                newSmartPlayList.Shuffle = (bool)cbShuffle.IsChecked == true;
                newSmartPlayList.Limit100 = (bool)cbLimit100.IsChecked == true;

                newSmartPlayList.Artists = new string[lbPerformers.Items.Count];
                for (int i = 0; i < (int)newSmartPlayList.Artists.Count(); i++)
                    newSmartPlayList.Artists[i] = (string)lbPerformers.Items.GetItemAt(i);

                newSmartPlayList.AlbumArtists = new string[lbAlbumArtists.Items.Count];
                for (int i = 0; i < (int)newSmartPlayList.AlbumArtists.Count(); i++)
                    newSmartPlayList.AlbumArtists[i] = (string)lbAlbumArtists.Items.GetItemAt(i);

                newSmartPlayList.Albums = new string[lbAlbums.Items.Count];
                for (int i = 0; i < (int)newSmartPlayList.Albums.Count(); i++)
                    newSmartPlayList.Albums[i] = (string)lbAlbums.Items.GetItemAt(i);
            }
            catch (Exception ex)
            {
                Log("Exception UpdateAndSaveSettings: " + ex.Message);
            }

            return newSmartPlayList;
        }

        private void UpdateAndSaveSmartPlaylist(string smartPlaylistName)
        {
            try
            {
                XmlSerializer serx = new XmlSerializer(typeof(SmartPlaylistCollection));
                TextWriter writerx = new StreamWriter(smartPlaylistName);
                serx.Serialize(writerx, currentPlaylistCollection);
                writerx.Close();
            }
            catch (Exception ex)
            {
                Log("Exception UpdateAndSaveSettings: " + ex.Message);
            }
        }

        private void LoadSettings(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                try
                {
                    // Read settinsg from file
                    XmlSerializer ser = new XmlSerializer(typeof(SmartPlaylistCollection));
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    currentPlaylistCollection = (SmartPlaylistCollection)ser.Deserialize(reader);

                    fs.Close();

                    comboPlaylist.Items.Clear();

                    foreach (SmartPlaylist curItem in currentPlaylistCollection.smartPlaylists )
                    {
                        comboPlaylist.Items.Add(curItem.Id);
                    }

                    tbMusicFolder.Text = currentPlaylistCollection.MusicRootFolder;

                    comboPlaylist.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    Log("Exception LoadSettings: " + ex.Message);
                }
            }
        }

        private SmartPlaylist GetSmartPlaylistFromCollectionById(String Id)
        {
            foreach (SmartPlaylist cur in currentPlaylistCollection.smartPlaylists )
            {
                if (cur.Id == Id)
                    return cur;
            }

            return null;
        }

        private int GetSmartPlaylistIndexFromCollectionById(String Id)
        {
            int i = 0;
            foreach (SmartPlaylist cur in currentPlaylistCollection.smartPlaylists)
            {
                if (cur.Id == Id)
                    return i;
                i++;
            }

            return -1;
        }

        private void LoadSmartPlaylistToUI(SmartPlaylist currentSmartPlaylist)
        {
            tbMusicFolder.Text = tbMusicFolder.Text;
            if (currentSmartPlaylist.Ratings != null)
            {
                cb1Star.IsChecked = currentSmartPlaylist.Ratings[0];
                cb2Star.IsChecked = currentSmartPlaylist.Ratings[1];
                cb3Star.IsChecked = currentSmartPlaylist.Ratings[2];
                cb4Star.IsChecked = currentSmartPlaylist.Ratings[3];
                cb5Star.IsChecked = currentSmartPlaylist.Ratings[4];
            }
            else
            {
                cb1Star.IsChecked = cb2Star.IsChecked = cb3Star.IsChecked = cb4Star.IsChecked = cb5Star.IsChecked = false;
            }

            cbShuffle.IsChecked = currentSmartPlaylist.Shuffle;
            cbLimit100.IsChecked = currentSmartPlaylist.Limit100;

            lbAlbumArtists.Items.Clear();

            if (currentSmartPlaylist.AlbumArtists != null && currentSmartPlaylist.AlbumArtists.Count() > 0)
            {
                for (int i = 0; i < currentSmartPlaylist.AlbumArtists.Count(); i++)
                    lbAlbumArtists.Items.Add(currentSmartPlaylist.AlbumArtists[i]);
            }

            lbPerformers.Items.Clear();

            if (currentSmartPlaylist.Artists != null && currentSmartPlaylist.Artists.Count() > 0)
            {
                for (int i = 0; i < currentSmartPlaylist.Artists.Count(); i++)
                    lbPerformers.Items.Add(currentSmartPlaylist.Artists[i]);
            }

            lbAlbums.Items.Clear();

            if (currentSmartPlaylist.Albums != null && currentSmartPlaylist.Albums.Count() > 0)
            {
                for (int i = 0; i < currentSmartPlaylist.Albums.Count(); i++)
                    lbAlbums.Items.Add(currentSmartPlaylist.Albums[i]);
            }

            tbPlaylistFile.Text = currentSmartPlaylist.PlaylistName;
            tbPlaylistId.Text = currentSmartPlaylist.Id;
        }

        private void btnAddArtist_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddPerformer.Text.Length > 0)
            {
                lbPerformers.Items.Add(tbAddPerformer.Text);
                tbAddPerformer.Text = String.Empty;
            }
        }

        private void MenuItem_SaveSmartPlaylist(object sender, RoutedEventArgs e)
        {

            // Save the current one before changing
            savePlaylistById(tbPlaylistId.Text);

            String selectedApp;
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            // fileDialog.InitialDirectory = tbMusicFolder.Text;
            fileDialog.Filter = "Smart Playlist Collection files (*.splc)|*.splc";
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    selectedApp = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    selectedApp = String.Empty;
                    break;

            }
            UpdateAndSaveSmartPlaylist(selectedApp);
        }

        private void savePlaylistById(String Id)
        {
            int newIndex = GetSmartPlaylistIndexFromCollectionById(Id);

            if (newIndex >= 0)
                currentPlaylistCollection.smartPlaylists[newIndex] = GetSmartPlaylistFromUI();
        }

        private void btnAddAlbumArtist_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddAlbumArtist.Text.Length > 0)
            {
                lbAlbumArtists.Items.Add(tbAddAlbumArtist.Text);
                tbAddAlbumArtist.Text = String.Empty;
            }
        }

        private void MenuItem_LoadSmartPlaylist(object sender, RoutedEventArgs e)
        {
            string selectedApp;
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.InitialDirectory = tbMusicFolder.Text;
            fileDialog.Filter = "Smart Playlist Collection files (*.splc)|*.splc";
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    selectedApp = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    selectedApp = String.Empty;
                    break;

            }

            LoadSettings(selectedApp);
            lblStatusInformation.Text = String.Format("Loaded {0}", selectedApp);
        }

        private void btnDelAlbumArtist_Click(object sender, RoutedEventArgs e)
        {
            if (lbAlbumArtists.SelectedItem == null)
                return;

            lbAlbumArtists.Items.RemoveAt(lbAlbumArtists.Items.IndexOf(lbAlbumArtists.SelectedItem));
            if ( lbAlbumArtists.Items.Count > 0 )
                lbAlbumArtists.SelectedItem = lbAlbumArtists.Items.IndexOf(0);
        }

        private void btnDelArtist_Click(object sender, RoutedEventArgs e)
        {
            if (lbPerformers.SelectedItem == null)
                return;

            lbPerformers.Items.RemoveAt(lbPerformers.Items.IndexOf(lbPerformers.SelectedItem));
            if ( lbPerformers.Items.Count > 0 )
                lbPerformers.SelectedItem = lbPerformers.Items.IndexOf(0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            comboPlaylist.Items.Add(Guid.NewGuid());
            comboPlaylist.SelectedIndex = comboPlaylist.Items.Count - 1;

            SmartPlaylist newPlaylistFromAdd = new SmartPlaylist();
            newPlaylistFromAdd.Id = comboPlaylist.SelectedItem.ToString();
            LoadSmartPlaylistToUI(newPlaylistFromAdd);
            currentPlaylistCollection.smartPlaylists.Add(newPlaylistFromAdd);

        }

        private void btnRemovePlaylist_Click(object sender, RoutedEventArgs e)
        {

            foreach(SmartPlaylist curPlaylist in currentPlaylistCollection.smartPlaylists )
            {
                if (curPlaylist.Id == comboPlaylist.SelectedItem.ToString() )
                {
                    currentPlaylistCollection.smartPlaylists.Remove(curPlaylist);
                    break;
                    // Found the one to remove
                }
            }

            comboPlaylist.Items.RemoveAt(comboPlaylist.Items.IndexOf(comboPlaylist.SelectedItem));
            comboPlaylist.SelectedIndex = 0;
        }

        private void btnSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            savePlaylistById(tbPlaylistId.Text);
        }

        private void comboPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This happens during a delete
            if (comboPlaylist.SelectedItem == null)
                return;

            // Save the current one before changing
            savePlaylistById(tbPlaylistId.Text);

            SmartPlaylist curPlaylist = GetSmartPlaylistFromCollectionById(comboPlaylist.SelectedItem.ToString());

            if (curPlaylist != null)
            {
                LoadSmartPlaylistToUI(curPlaylist);
            }
        }

        private void btnDelAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (lbAlbums.SelectedItem == null)
                return;

            lbAlbums.Items.RemoveAt(lbAlbums.Items.IndexOf(lbAlbums.SelectedItem));
            if (lbAlbums.Items.Count > 0)
                lbAlbums.SelectedItem = lbAlbums.Items.IndexOf(0);

        }

        private void btnAddAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddAlbum.Text.Length > 0)
            {
                lbAlbums.Items.Add(tbAddAlbum.Text);
                tbAddAlbum.Text = String.Empty;
            }

        }
    }
}