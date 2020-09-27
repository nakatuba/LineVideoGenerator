using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LineVideoGenerator
{
    /// <summary>
    /// BackgroundWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BackgroundWindow : Window
    {
        public BackgroundWindow()
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
                    mainWindow.backgroundType = BackgroundType.Image;
                    mainWindow.backgroundImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                    mainWindow.mediaElement.Source = null;

                    resetButton.IsEnabled = true;
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
                    mainWindow.backgroundType = BackgroundType.Animation;
                    mainWindow.backgroundImage.Source = null;
                    mainWindow.mediaElement.Source = new Uri(openFileDialog.FileName);
                    mainWindow.mediaElement.Stop();

                    resetButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.backgroundType = BackgroundType.Default;
            mainWindow.backgroundImage.Source = new BitmapImage(new Uri("background.png", UriKind.Relative));
            mainWindow.mediaElement.Source = null;

            resetButton.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.backgroundButton.IsEnabled = true;
        }
    }
}
