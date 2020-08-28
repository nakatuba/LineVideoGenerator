using Microsoft.Win32;
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
                    mainWindow.backgroundPath = openFileDialog.FileName;
                    mainWindow.isAnimated = false;

                    BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    mainWindow.backgroundImage.Source = bitmapImage;
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
                    mainWindow.backgroundPath = openFileDialog.FileName;
                    mainWindow.isAnimated = true;

                    mainWindow.mediaElement.Source = new Uri(openFileDialog.FileName);
                    mainWindow.backgroundImage.Source = null;

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
            mainWindow.backgroundPath = "background.png";
            mainWindow.isAnimated = false;

            BitmapImage bitmapImage = new BitmapImage(new Uri(mainWindow.backgroundPath, UriKind.Relative));
            mainWindow.backgroundImage.Source = bitmapImage;
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
