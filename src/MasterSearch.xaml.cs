using BallparkAudioDashboard.DataContracts;
using BallparkAudioDashboard.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BallparkAudioDashboard
{
    /// <summary>
    /// Interaction logic for MasterSearch.xaml
    /// </summary>
    public partial class MasterSearch : Window
    {
        public const string SONG_SEARCH_TEXTBOX_DEFAULT_TEXT = "Search...";
        private IEnumerable<Song> _allSongs = null;
        private MainWindow _mainWindow = null;
        private AudioFilesServices _audioFilesServices = null;

        public MasterSearch(MainWindow mainWindow, IEnumerable<Song> allSongs)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _allSongs = allSongs;
            _audioFilesServices = new AudioFilesServices();
            Owner = Application.Current.MainWindow;

            this.PreviewKeyDown += new KeyEventHandler(MasterSearch_KeyEventHandler);
        }

        private void MasterSearch_KeyEventHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Search();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == SONG_SEARCH_TEXTBOX_DEFAULT_TEXT)
            {
                SearchTextBox.Text = string.Empty;
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = null;
            SearchTextBox.Focus();

            SearchResultListView.ItemsSource = null;
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == string.Empty)
            {
                SearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            }
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchResultListView.ItemsSource = null;
            }
            else
            {
                IEnumerable<Song> resultantSongs = _allSongs.Where(song => _audioFilesServices.IsSongAMatch(song.Title, SearchTextBox.Text));
                SearchResultListView.ItemsSource = new ObservableCollection<Song>(resultantSongs);
            }
        }

        private void SearchResultListView_AddToQueue(object sender, RoutedEventArgs e)
        {
            _mainWindow.AddSongToQueue(SearchResultListView);
            this.DialogResult = false;
        }

        private void SearchResultListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            _mainWindow.LoadInPlayerMediaElement(SearchResultListView);
        }

        private void SearchResultListView_LoadInPlayer2(object sender, RoutedEventArgs e)
        {
            _mainWindow.LoadInPlayerMediaElement2(SearchResultListView);
        }

        private void SearchResultListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchResultListView.SelectedItem != null)
            {
                _mainWindow.PlayInPlayerMediaElement(SearchResultListView);
                this.DialogResult = false;
            }
        }

        #region Result Actions
        private void PlayInOne_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PlayInPlayerMediaElement(SearchResultListView);
            this.DialogResult = false;
        }

        private void PlayInTwo_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PlayInPlayerMediaElement2(SearchResultListView);
            this.DialogResult = false;
        }

        private void LoadInOne_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.LoadInPlayerMediaElement(SearchResultListView);
        }

        private void LoadInTwo_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.LoadInPlayerMediaElement2(SearchResultListView);
        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.AddSongToQueue(SearchResultListView);
            this.DialogResult = false;
        }
        #endregion Rsult Actions

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
