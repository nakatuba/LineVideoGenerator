using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace LineVideoGenerator
{
    /// <summary>
    /// BackgroundWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    mainWindow.backgroundPath = openFileDialog.FileName;
                    mainWindow.backgroundType = BackgroundType.Image;

                    BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    mainWindow.backgroundImage.Source = bitmapImage;
                    mainWindow.mediaElement.Source = null;

                    resetBackgroundButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void AnimationButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    mainWindow.backgroundPath = openFileDialog.FileName;
                    mainWindow.backgroundType = BackgroundType.Animation;

                    mainWindow.backgroundImage.Source = null;
                    mainWindow.mediaElement.Source = new Uri(openFileDialog.FileName);

                    resetBackgroundButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void ResetBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.backgroundPath = "background.png";
            mainWindow.backgroundType = BackgroundType.Default;

            BitmapImage bitmapImage = new BitmapImage(new Uri("background.png", UriKind.Relative));
            mainWindow.backgroundImage.Source = bitmapImage;
            mainWindow.mediaElement.Source = null;

            resetBackgroundButton.IsEnabled = false;
        }

        private void BGMButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    mainWindow.bgm = new AudioFileReader(openFileDialog.FileName);

                    resetBGMButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void ResetBGMButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.bgm = null;

            resetBGMButton.IsEnabled = false;
        }

        private void SEButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    mainWindow.soundEffect = new AudioFileReader(openFileDialog.FileName);

                    resetSEButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void ResetSEButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.soundEffect = null;

            resetSEButton.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.backgroundButton.IsEnabled = true;
        }
    }
}
