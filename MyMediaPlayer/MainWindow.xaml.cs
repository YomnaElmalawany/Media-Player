using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog dialog;

        MediaPlayer player;
        Uri mediaUri;

        int playFlag = 0;
        int endFlag = 1;

        DispatcherTimer timer;

        List<Uri> audioList = new List<Uri>();

        string nowPlaying;
        public MainWindow()
        {
            InitializeComponent();

            dialog = new OpenFileDialog();
            //dialog.Filter = "(mp3,wav,mp4,mov,wmv,mpg,3gp,m4a)|*.mp3;*.wav;*.mp4;*.mov;*.wmv;*.mpg;*.3gp;*.m4a|all files|*.*";
            dialog.Multiselect = true;

            player = new MediaPlayer();

            /*Initializing timer*/
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;

            /*Initializing the Volume*/
            player.Volume = 0.3;
            volumeSlider.Value = player.Volume;
            volumeLabel.Content = Math.Truncate((player.Volume / volumeSlider.Maximum) * 100) + "%";

            /*handling opened and ended events*/
            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;

            /*initial state of the buttons (disabled until a media is selected)*/
            playBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
            FastforwardBtn.IsEnabled = false;
            RewindBtn.IsEnabled = false;

            /*Initializing time labels*/
            TotalTimeLabel.Content = "--:--";
            ElapsedTimeLabel.Content = "--:--";

        }
        private void Player_MediaEnded(object sender, EventArgs e)
        {
            ProgressSlider.Value = 0;
            
            int oldIndex = MyListBox.Items.IndexOf(nowPlaying);
            if(oldIndex < MyListBox.Items.Count && endFlag == 1)
            {
                if (oldIndex == MyListBox.Items.Count - 1)
                {
                    player.Stop();
                    //mediaElement.LoadedBehavior = MediaState.Stop;
                    playFlag = 0;
                    playBtn.Background = new ImageBrush(new BitmapImage(new Uri("../../Images/play.png", UriKind.Relative)));
                    endFlag = 0;
                }
                else
                {
                    nowPlaying = MyListBox.Items[oldIndex + 1].ToString();
                    this.Title = nowPlaying;

                    player.Open(audioList[oldIndex + 1]);
                    //if (nowPlaying.Split('.')[1] == "mp4" || nowPlaying.Split('.')[1] == "3gp")
                    //{
                    //    mediaElement.Source = new Uri("../../music/" + nowPlaying, UriKind.Relative);
                    //    //mediaElement.LoadedBehavior = MediaState.Play;
                    //}
                    //else
                    //    player.Play();
                    player.Play();

                    /*Starts timer and handles progress on horizontal slider */
                    HandleProgressBar();

                    MyListBox.SelectedItem = nowPlaying;

                    this.Icon = new BitmapImage(new Uri("../../Images/" + nowPlaying.Split('.')[1] + ".png", UriKind.Relative));

                }

            }

        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            /*
             * buttons will be enabled only when the media is loaded (opened).. 
             * to prevent an exception of NaturalDuration doesnot exist
             */
            playBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;
            FastforwardBtn.IsEnabled = true;
            RewindBtn.IsEnabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ProgressSlider.Value = player.Position.TotalMilliseconds;

            TimeSpan currentTime = player.Position;

            /*splitting time to hours, mins, secs to be displayed in label*/
            ElapsedTimeLabel.Content = currentTime.Hours + ":" + currentTime.Minutes + ":" + currentTime.Seconds;

            TaskbarManager.Instance.SetProgressValue((int)player.Position.TotalSeconds, (int)player.NaturalDuration.TimeSpan.TotalSeconds);
        
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            /*Starts timer and handles progress on horizontal slider */
            HandleProgressBar();

            string imageUri;

            if (playFlag == 0)
            {
                //if (nowPlaying.Split('.')[1] == "mp4" || nowPlaying.Split('.')[1] == "3gp")
                //{
                //    mediaElement.Source = new Uri("../../music/" + nowPlaying, UriKind.Relative);
                //    mediaElement.LoadedBehavior = MediaState.Play;
                //}
                //else
                //    player.Play();
                player.Play();

                player.Volume = (double)volumeSlider.Value;

                string dir = System.IO.Directory.GetCurrentDirectory();
                imageUri = "../../Images/pause.png";

                playFlag = 1;

                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);

            }
            else
            {
                player.Pause();
                //mediaElement.LoadedBehavior = MediaState.Pause;
                player.Volume = (double)volumeSlider.Value;

                imageUri = "../../Images/play.png";

                playFlag = 0;

                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
            }

            playBtn.Background = new ImageBrush(new BitmapImage(new Uri(imageUri, UriKind.Relative)));

        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            //mediaElement.LoadedBehavior = MediaState.Stop;

            timer.Stop();

            playFlag = 0;
            string imageUri = "../../Images/play.png";
            playBtn.Background = new ImageBrush(new BitmapImage(new Uri(imageUri, UriKind.Relative)));

            ElapsedTimeLabel.Content = "0:0:0";
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            if((bool)dialog.ShowDialog())
            {
                timer.Stop();
                mediaUri = new Uri(dialog.FileName);
                player.Open(mediaUri);
                nowPlaying = dialog.SafeFileName;
                this.Title = nowPlaying;
                /*if a new media is selected while playing the old one, it starts running the new*/
                playFlag = 0;
                string imageUri = "../../Images/play.png";
                playBtn.Background = new ImageBrush(new BitmapImage(new Uri(imageUri, UriKind.Relative)));

                /*add the currently playing audio to the listbox*/
                audioList.Add(new Uri(dialog.FileName));
                MyListBox.Items.Add(dialog.SafeFileName);

                MyListBox.SelectedItem = dialog.SafeFileName;

                this.Icon = new BitmapImage(new Uri("../../Images/" + dialog.SafeFileName.Split('.')[1] + ".png", UriKind.Relative));
                
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            volumeLabel.Content = Math.Truncate((volumeSlider.Value / volumeSlider.Maximum) * 100) + "%";
            player.Volume = (double)volumeSlider.Value;
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Position = TimeSpan.FromMilliseconds(ProgressSlider.Value);
        }

        private void ProgressSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            player.Position = TimeSpan.FromMilliseconds(ProgressSlider.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        int flag = 0;
        private void CollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            string imageUri;
            if(flag == 0) 
            {
                MyListBox.Width = 0;
                flag = 1;
                imageUri = "../../Images/expand.png";
                CollapseBtn.ToolTip = "Expand list";

                this.Width = 600;
            }
            else
            {
                MyListBox.Width = 172;
                flag = 0;
                imageUri = "../../Images/collapse.png";
                CollapseBtn.ToolTip = "Collapse list";

                this.Width = 785;
            }
            CollapseBtn.Background = new ImageBrush(new BitmapImage(new Uri(imageUri, UriKind.Relative)));
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)dialog.ShowDialog())
            {
                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    audioList.Add(new Uri(dialog.FileNames[i]));
                    MyListBox.Items.Add(dialog.SafeFileNames[i]);
                }
                
            }
        }

        private void FastforwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (player.Position <= player.NaturalDuration.TimeSpan - TimeSpan.FromSeconds(10))
                player.Position += TimeSpan.FromSeconds(10);
        }

        private void RewindBtn_Click(object sender, RoutedEventArgs e)
        {
            if(player.Position >= TimeSpan.FromSeconds(10))
                player.Position -= TimeSpan.FromSeconds(10);
            
        }

        private void MyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            player.Open(audioList[MyListBox.SelectedIndex]);
            nowPlaying = MyListBox.SelectedItem.ToString();
            this.Title = nowPlaying;

            //if (nowPlaying.Split('.')[1] == "mp4" || nowPlaying.Split('.')[1] == "3gp")
            //{
            //    mediaElement.Source = new Uri("../../music/" + nowPlaying, UriKind.Relative);
            //    mediaElement.LoadedBehavior = MediaState.Play;
            //}
            //else
            //    player.Play();
            player.Play();

            playFlag = 1;
            string imageUri = "../../Images/pause.png";
            playBtn.Background = new ImageBrush(new BitmapImage(new Uri(imageUri, UriKind.Relative)));

            /*Starts timer and handles progress on horizontal slider */
            HandleProgressBar();

            this.Icon = new BitmapImage(new Uri("../../Images/" + nowPlaying.Split('.')[1] + ".png", UriKind.Relative));
        }
        private bool IsOpened()
        {
            bool checkFlag = true;
            bool retVal = false;
            while (checkFlag)
            {
                if (player.NaturalDuration.HasTimeSpan)
                {
                    checkFlag = false;
                    retVal = true;
                }
            }
            return retVal;
        }

        private void HandleProgressBar()
        {
            timer.Stop();
            timer.Start();

            if (IsOpened())
            {
                /*setting some values depending in the selectd audio*/
                TimeSpan totaltTime = player.NaturalDuration.TimeSpan;
                ProgressSlider.Maximum = totaltTime.TotalMilliseconds;

                /*splitting time to hours, mins, secs to be displayed in label*/
                TotalTimeLabel.Content = totaltTime.Hours + ":" + totaltTime.Minutes + ":" + totaltTime.Seconds;
            }
            
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if(MyListBox.SelectedIndex != -1)
            {
                audioList.Remove(audioList[MyListBox.SelectedIndex]);
                MyListBox.Items.Remove(MyListBox.SelectedItem);
            }
            else
            {
                MessageBox.Show("No item is selected!");
            }
            
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            MyListBox.Items.Clear();
            audioList.Clear();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ParentGrid.Width = this.Width;
            //ParentGrid.Height = this.Height;

            //Grid2.Width = this.Width;
            //Grid2.Height = this.Height;

            //MainMenu.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
