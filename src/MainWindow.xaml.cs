using BallparkAudioDashboard.DataContracts;
using BallparkAudioDashboard.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BallparkAudioDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioFilesServices _audioFilesServices = null;
        private QueueServices _queueServices = null;
        private string _loadedQueueName = string.Empty;
        private ObservableCollection<Song> _fullLengthSongs = null;
        private IEnumerable<Song> _soundClips = null;
        private bool _userIsDraggingSlider = false;
        private static readonly Random randomSongShuffler = new Random();
        private bool _playAllFullLengthSongs = false;
        private bool _playAllQueuedSongs = false;
        private readonly DispatcherTimer _fadeOutTimer = new DispatcherTimer();

        private const string SONG_SEARCH_TEXTBOX_DEFAULT_TEXT = "Search...";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _audioFilesServices = new AudioFilesServices();
            _queueServices = new QueueServices();

            SongsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            SoundClipsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;

            QueueSongsListView.ItemsSource = new ObservableCollection<Song>();

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += timer_Tick;
            timer.Start();

            _fadeOutTimer.Interval = TimeSpan.FromSeconds(4);
            _fadeOutTimer.Tick += _fadeOutTimer_Tick;

            PlayerMediaElement.BeginAnimation(MediaElement.VolumeProperty, new DoubleAnimation(PlayerMediaElement.Volume, 1, TimeSpan.FromSeconds(1)));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((PlayerMediaElement.Source != null) && (PlayerMediaElement.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                SongSlider.Minimum = 0;
                SongSlider.Maximum = PlayerMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                SongSlider.Value = PlayerMediaElement.Position.TotalSeconds;
                SongCountDownLabel.Content = PlayerMediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        private void FullSongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            _fullLengthSongs = new ObservableCollection<Song>(_audioFilesServices.GetFullLengthSongs());
            FullSongsListView.ItemsSource = _fullLengthSongs;
        }

        private void TraditionSongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView fullSongsListView = sender as ListView;
            fullSongsListView.ItemsSource = _audioFilesServices.GetTraditionSongs();
        }

        private void OrganSongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView fullSongsListView = sender as ListView;
            fullSongsListView.ItemsSource = _audioFilesServices.GetOrganSongs();
        }

        private void SoundClipListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView soundClipListView = sender as ListView;
            _soundClips = _audioFilesServices.GetSoundClips();
            foreach (Song song in _soundClips)
            {
                soundClipListView.Items.Add(song);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement originalSource = e.OriginalSource as FrameworkElement;
            FrameworkElement source = e.Source as FrameworkElement;

            if (originalSource.DataContext != source.DataContext)
            {
                Song song = ((ListView)sender).SelectedItem as Song;
                PlayerMediaElement.BeginAnimation(MediaElement.VolumeProperty, new DoubleAnimation(PlayerMediaElement.Volume, 1, TimeSpan.FromSeconds(1)));
                PlayerMediaElement.Source = new Uri(song.FullPath);
                PlayerMediaElement.Play();
                _playAllFullLengthSongs = false;
                _playAllQueuedSongs = false;
            }
        }

        private void TraditionSongsAddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(TraditionSongsListView);
        }

        private void OrganSongsAddToQueueButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(OrganSongsListView);
        }

        private void FullSongsAddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(FullSongsListView);
        }

        private void SoundClipsAddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(SoundClipListView);
        }

        private void AddSongToQueue(ListView listView)
        {
            foreach (Song song in listView.SelectedItems)
            {
                if (!IsInQueue(song))
                {
                    List<Song> songs = QueueSongsListView.ItemsSource.Cast<Song>().ToList();
                    songs.Add(song);
                    QueueSongsListView.ItemsSource = new ObservableCollection<Song>(songs);
                }
            }
        }

        private bool IsInQueue(Song song)
        {
            foreach (Song queuedSong in QueueSongsListView.Items)
            {
                if (queuedSong.Title == song.Title) { return true; }
            }

            return false;
        }

        private void ClearAllQueueButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to clear the Playlist?", "Clear Playlist", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                QueueSongsListView.ItemsSource = new ObservableCollection<Song>();
                SavedQueuesComboBox.SelectedIndex = -1;
            }
        }

        private void ClearSelectedQueueButton_Click(object sender, RoutedEventArgs e)
        {
            List<Song> songsToRemove = QueueSongsListView.SelectedItems.Cast<Song>().ToList();
            List<Song> songs = QueueSongsListView.ItemsSource.Cast<Song>().ToList();
            List<Song> resultantSong = new List<Song>();
            foreach (Song song in songs)
            {
                if (!songsToRemove.Any(s => s.Title == song.Title))
                {
                    resultantSong.Add(song);
                }
            }

            QueueSongsListView.ItemsSource = new ObservableCollection<Song>(resultantSong);
        }

        private void SaveQueueButton_Click(object sender, RoutedEventArgs e)
        {
            string queueName = Microsoft.VisualBasic.Interaction.InputBox("Save..", "Enter name of Playlist.", _loadedQueueName);
            if (!string.IsNullOrEmpty(queueName))
            {
                _queueServices.Create(queueName, QueueSongsListView.Items.Cast<Song>());
                SavedQueuesComboBox.ItemsSource = _queueServices.Get();
                SavedQueuesComboBox.SelectedValue = queueName;
                _loadedQueueName = queueName;
            }
        }

        private void SaveAsQueueButton_Click(object sender, RoutedEventArgs e)
        {
            string queueName = Microsoft.VisualBasic.Interaction.InputBox("Save as..", "Enter name of Playlist.", _loadedQueueName);
            if (!string.IsNullOrEmpty(queueName))
            {
                _queueServices.Create(queueName, QueueSongsListView.Items.Cast<Song>());
                SavedQueuesComboBox.ItemsSource = _queueServices.Get();
                SavedQueuesComboBox.SelectedValue = queueName;
                _loadedQueueName = queueName;
            }
        }

        private void UnloadSavedQueue_Click(object sender, RoutedEventArgs e)
        {
            SavedQueuesComboBox.SelectedIndex = -1;
            _loadedQueueName = string.Empty;
            QueueSongsListView.ItemsSource = new ObservableCollection<Song>();

            UnloadSavedQueue.IsEnabled = false;
        }

        private void SavedQueuesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _loadedQueueName = ((ComboBox)sender).SelectedItem as string;
            if (_loadedQueueName != null)
            {
                QueueSongsListView.ItemsSource = _queueServices.Get(_loadedQueueName);
                UnloadSavedQueue.IsEnabled = true;
            }
        }

        private void SavedQueuesComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            SavedQueuesComboBox.ItemsSource = null;
            SavedQueuesComboBox.ItemsSource = _queueServices.Get();
        }

        private void FadeInButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerMediaElement.Volume = 0;
            PlayerMediaElement.BeginAnimation(MediaElement.VolumeProperty, new DoubleAnimation(PlayerMediaElement.Volume, 1, TimeSpan.FromSeconds(4)));
            PlayerMediaElement.Play();

        }

        private void FadeOutButton_Click(object sender, RoutedEventArgs e)
        {
            _fadeOutTimer.Start();
            PlayerMediaElement.BeginAnimation(MediaElement.VolumeProperty, new DoubleAnimation(PlayerMediaElement.Volume, 0, TimeSpan.FromSeconds(3)));
        }

        private void _fadeOutTimer_Tick(object sender, EventArgs e)
        {
            _fadeOutTimer.Stop();
            PlayerMediaElement.Pause();
            PlayerMediaElement.Volume = 1;

            _playAllFullLengthSongs = false;
            _playAllQueuedSongs = false;
        }

        private void SongsSearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            IEnumerable<Song> resultantSongs = _fullLengthSongs.Where(song => song.Title.IndexOf(SongsSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            FullSongsListView.ItemsSource = new ObservableCollection<Song>(resultantSongs);

            if (SongsSearchTextBox.Text != string.Empty)
            {
                SongsSearchTextBox.Background = Brushes.Yellow;
            }
        }

        private void SongsSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SongsSearchTextBox.Text == SONG_SEARCH_TEXTBOX_DEFAULT_TEXT)
            {
                SongsSearchTextBox.Text = string.Empty;
            }
        }

        private void SongsSearchClearButton_Click(object sender, RoutedEventArgs e)
        {
            FullSongsListView.ItemsSource = _fullLengthSongs;

            SongsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            SongsSearchTextBox.Background = Brushes.White;
        }

        private void SongsSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SongsSearchTextBox.Text == string.Empty)
            {
                SongsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
                SongsSearchTextBox.Background = Brushes.White;
            }
        }

        private void SongSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void SongSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            PlayerMediaElement.Position = TimeSpan.FromSeconds(SongSlider.Value);
        }

        private void SongSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SongCountUpLabel.Content = TimeSpan.FromSeconds(SongSlider.Value).ToString(@"mm\:ss");
        }

        private void SoundClipsSearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            IEnumerable<Song> resultantSoundClips = _soundClips.Where(song => song.Title.IndexOf(SoundClipsSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            SoundClipListView.Items.Clear();
            foreach (Song song in resultantSoundClips)
            {
                SoundClipListView.Items.Add(song);
            }

            if (SoundClipsSearchTextBox.Text != string.Empty)
            {
                SoundClipsSearchTextBox.Background = Brushes.Yellow;
            }
        }

        private void SoundClipsSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SoundClipsSearchTextBox.Text == SONG_SEARCH_TEXTBOX_DEFAULT_TEXT)
            {
                SoundClipsSearchTextBox.Text = string.Empty;
            }
        }

        private void SoundClipsSearchClearButton_Click(object sender, RoutedEventArgs e)
        {
            SoundClipsSearchTextBox.Text = string.Empty;
            IEnumerable<Song> resultantSoundClips = _soundClips.Where(song => song.Title.IndexOf(SoundClipsSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            SoundClipListView.Items.Clear();
            foreach (Song soundClip in resultantSoundClips)
            {
                SoundClipListView.Items.Add(soundClip);
            }

            SoundClipsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            SoundClipsSearchTextBox.Background = Brushes.White;
        }

        private void SoundClipsSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SoundClipsSearchTextBox.Text == string.Empty)
            {
                SoundClipsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
                SoundClipsSearchTextBox.Background = Brushes.White;
            }
        }

        private void FullLengthSongsSortButton_Click(object sender, RoutedEventArgs e)
        {
            _fullLengthSongs = new ObservableCollection<Song>(_audioFilesServices.GetFullLengthSongs());
            FullSongsListView.ItemsSource = _fullLengthSongs;

            SongsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            SongsSearchTextBox.Background = Brushes.White;
        }

        public static void ShuffleSongs(ObservableCollection<Song> song)
        {
            int n = song.Count;
            while (n > 1)
            {
                int k = (randomSongShuffler.Next(0, n) % n);
                n--;
                Song value = song[k];
                song[k] = song[n];
                song[n] = value;
            }
        }

        private void FullLengthSongsShuffleAndPlayButton_Click(object sender, RoutedEventArgs e)
        {
            SongsSearchTextBox.Text = SONG_SEARCH_TEXTBOX_DEFAULT_TEXT;
            _fullLengthSongs = new ObservableCollection<Song>(_audioFilesServices.GetFullLengthSongs());
            ShuffleSongs(_fullLengthSongs);
            FullSongsListView.ItemsSource = _fullLengthSongs;

            PlayerMediaElement.Source = new Uri(FullSongsListView.Items.Cast<Song>().First().FullPath);
            PlayerMediaElement.Play();
            FullSongsListView.SelectedIndex = 0;

            _playAllFullLengthSongs = true;
            _playAllQueuedSongs = false;
        }

        private void PlayerMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Song nextSong = null;

            if (_playAllFullLengthSongs && _fullLengthSongs.Any())
            {
                int nextSongIndex = 0;
                for (int i = 0; i < _fullLengthSongs.Count(); i++)
                {
                    if (_fullLengthSongs[i].FullPath == PlayerMediaElement.Source.OriginalString)
                    {
                        nextSongIndex = i + 1;
                        if (nextSongIndex >= _fullLengthSongs.Count())
                        {
                            nextSongIndex = 0;
                        }
                        break;
                    }
                }

                nextSong = _fullLengthSongs[nextSongIndex];
                FullSongsListView.SelectedIndex = nextSongIndex;
            }

            if (_playAllQueuedSongs && QueueSongsListView.HasItems)
            {
                List<Song> queuedSongs = QueueSongsListView.Items.Cast<Song>().ToList();
                int nextSongIndex = 0;
                for (int i = 0; i < queuedSongs.Count(); i++)
                {
                    if (queuedSongs[i].FullPath == PlayerMediaElement.Source.OriginalString)
                    {
                        nextSongIndex = i + 1;
                        if (nextSongIndex >= queuedSongs.Count())
                        {
                            nextSongIndex = 0;
                        }
                        break;
                    }
                }

                nextSong = queuedSongs[nextSongIndex];
                QueueSongsListView.SelectedIndex = nextSongIndex;
            }

            if (nextSong != null)
            {
                PlayerMediaElement.Source = new Uri(nextSong.FullPath);
                PlayerMediaElement.Play();
            }
        }

        private void PlayerMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            FileInfo songFileInfo = new FileInfo(PlayerMediaElement.Source.OriginalString);
            SongTitleLabel.Content = songFileInfo.Name.Replace(songFileInfo.Extension, string.Empty);
        }

        private void SongSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlayerMediaElement.Position = TimeSpan.FromSeconds(SongSlider.Value);
        }

        private void LoadInPlayerMediaElement(ListView listView)
        {
            if (listView.SelectedIndex == -1) return;

            Song song = listView.SelectedItem as Song;
            PlayerMediaElement.Source = new Uri(song.FullPath);
            PlayerMediaElement.Play();
            PlayerMediaElement.Pause();
            PlayerMediaElement.BeginAnimation(MediaElement.VolumeProperty, new DoubleAnimation(PlayerMediaElement.Volume, 0, TimeSpan.FromSeconds(1)));
        }

        private void FullSongsListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            LoadInPlayerMediaElement(FullSongsListView);
        }

        private void QueueSongsListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            LoadInPlayerMediaElement(QueueSongsListView);
        }

        private void TraditionSongsListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            LoadInPlayerMediaElement(TraditionSongsListView);
        }

        private void OrganSongsListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            LoadInPlayerMediaElement(OrganSongsListView);
        }

        private void QueueSongsListViewPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (QueueSongsListView.HasItems)
            {
                PlayerMediaElement.Source = new Uri(QueueSongsListView.Items.Cast<Song>().First().FullPath);
                PlayerMediaElement.Play();
                QueueSongsListView.SelectedIndex = 0;

                _playAllFullLengthSongs = false;
                _playAllQueuedSongs = true;
            }
        }

        private void QueueSongsListViewShuffleAndPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (QueueSongsListView.HasItems)
            {
                ObservableCollection<Song> queueSongsList = new ObservableCollection<Song>(QueueSongsListView.ItemsSource.Cast<Song>());
                ShuffleSongs(queueSongsList);
                QueueSongsListView.ItemsSource = queueSongsList;

                PlayerMediaElement.Source = new Uri(QueueSongsListView.Items.Cast<Song>().First().FullPath);
                PlayerMediaElement.Play();
                QueueSongsListView.SelectedIndex = 0;

                _playAllFullLengthSongs = false;
                _playAllQueuedSongs = true;
            }
        }

        private void FullSongsListView_AddToQueue(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(FullSongsListView);
        }

        private void TraditionSongsListView_AddToQueue(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(TraditionSongsListView);
        }

        private void OrganSongsListView_AddToQueue(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(OrganSongsListView);
        }

        private void SoundClipListView_AddToQueue(object sender, RoutedEventArgs e)
        {
            AddSongToQueue(SoundClipListView);
        }

        #region PA Tab
        private IEnumerable<Player> GetPlayers()
        {
            string filePath = GetFilePath("PlayersFilePath");

            if (File.Exists(filePath))
            {
                List<Player> players = new List<Player>();
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] playerLine = line.Split('|');
                        players.Add(new Player
                        {
                            DisplayName = playerLine[0],
                            Number = playerLine[1],
                            SpeechName = playerLine[2]
                        });
                    }

                    return players;
                }
            }

            return new List<Player>();
        }

        private IEnumerable<Position> GetPositions()
        {
            string filePath = GetFilePath("PositionsFilePath");

            if (File.Exists(filePath))
            {
                List<Position> positions = new List<Position>();
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] positionLine = line.Split('|');
                        positions.Add(new Position
                        {
                            SpeechPosition = positionLine[0],
                            PositionName = positionLine[1]
                        });
                    }
                }

                return positions;
            }

            return new List<Position>();
        }

        private void cmbPlayers_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox playerCombobox = sender as ComboBox;
            playerCombobox.ItemsSource = GetPlayers();
            playerCombobox.DisplayMemberPath = "DisplayName";
            playerCombobox.SelectedIndex = 0;
        }

        private void cmbPositions_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox playerListBox = sender as ComboBox;
            playerListBox.ItemsSource = GetPositions();
            playerListBox.DisplayMemberPath = "PositionName";
            playerListBox.SelectedIndex = 0;
        }

        private void btnSayIt_Click(object sender, RoutedEventArgs e)
        {
            Player player = cmbPlayers.SelectedItem as Player;
            Position position = cmbPositions.SelectedItem as Position;
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Speak(position.SpeechPosition + " number " + player.Number + "! " + player.SpeechName);
        }

        private string GetFilePath(string configurationKey)
        {
            if (ConfigurationManager.AppSettings.Get(configurationKey) == null) throw new Exception(string.Format("Configuration missing \"{0}\" in AppSettings. Add to App.config", configurationKey));
            return ConfigurationManager.AppSettings.Get(configurationKey);
        }

        private void FreeFormButton_Click(object sender, RoutedEventArgs e)
        {
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Speak(FreeFormTextBox.Text);
        }
        #endregion

        private void WalkUpSongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            WalkUpSongsListView.ItemsSource = new ObservableCollection<Song>(_audioFilesServices.GetWalkUpSongs());
        }

        private void WalkUpSongsListView_LoadInPlayer(object sender, RoutedEventArgs e)
        {
            LoadInPlayerMediaElement(WalkUpSongsListView);

        }
    }
}
