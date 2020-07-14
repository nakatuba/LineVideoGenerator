using AviFile;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Security.Policy;
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
using System.Windows.Threading;
using Point = System.Windows.Point;
using Brushes = System.Windows.Media.Brushes;

namespace line
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<(BitmapImage icon, string name, string text)> messageList = new List<(BitmapImage icon, string name, string text)>();
        private const double messageBlockDistance = 50;
        private double messageBlockTop = messageBlockDistance;
        private double messageBlockSide = 30;
        private int frameRate = 1;
        private List<Bitmap> bitmapList = new List<Bitmap>();
        private TimeSpan soundTimeSpan = TimeSpan.FromSeconds(1);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = new EditWindow();
            editWindow.Owner = this;
            editWindow.Show();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            editButton.IsEnabled = false;
            playButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            grid.Children.Clear();
            bitmapList.Clear();
            messageBlockTop = messageBlockDistance;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / frameRate);
            dispatcherTimer.Tick += AddBitmapList;
            dispatcherTimer.Start();

            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();
            for (int i = 0; i < messageList.Count; i++)
            {
                AudioFileReader audioFileReader = new AudioFileReader("send.wav");
                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(audioFileReader);
                if(i == 0)
                {
                    await Task.Delay(soundTimeSpan);
                    offsetSampleProvider.DelayBy = soundTimeSpan;
                }
                offsetSampleProvider.LeadOut = soundTimeSpan;
                sampleProviderList.Add(offsetSampleProvider);

                SendMessage(messageList[i].icon, messageList[i].name, messageList[i].text);
                await Task.Delay(audioFileReader.TotalTime + soundTimeSpan);
            }
            ConcatenatingSampleProvider concatenatingSampleProvider = new ConcatenatingSampleProvider(sampleProviderList);
            WaveFileWriter.CreateWaveFile16("temp.wav", concatenatingSampleProvider);

            dispatcherTimer.Stop();
            editButton.IsEnabled = true;
            playButton.IsEnabled = true;
            saveButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".avi";

            if (saveFileDialog.ShowDialog() == true)
            {
                AviManager aviManager = new AviManager(saveFileDialog.FileName, false);
                VideoStream aviStream = aviManager.AddVideoStream(false, frameRate, bitmapList[0]);
                for (int i = 1; i < bitmapList.Count; i++)
                {
                    aviStream.AddFrame(bitmapList[i]);
                }
                aviManager.AddAudioStream("temp.wav", 0);
                aviManager.Close();
            }
        }

        private void AddBitmapList(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap((int)grid.Width * 2, (int)grid.Height * 2);
            Graphics graphics = Graphics.FromImage(bitmap);
            int gridX = (int)grid.PointToScreen(new Point(0, 0)).X;
            int gridY = (int)grid.PointToScreen(new Point(0, 0)).Y;
            graphics.CopyFromScreen(new System.Drawing.Point(gridX, gridY), System.Drawing.Point.Empty, bitmap.Size);
            bitmapList.Add(bitmap);
        }

        public void SendMessage(BitmapImage icon, string name, string text)
        {
            TextBlock messageBlock = new TextBlock();
            messageBlock.Text = text;
            messageBlock.FontSize = 30;
            messageBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.MaxWidth = grid.Width / 2;
            messageBlock.Margin = new Thickness(20, 10, 20, 10);

            Border border = new Border();
            border.Child = messageBlock;
            border.VerticalAlignment = VerticalAlignment.Top;
            border.CornerRadius = new CornerRadius(20);
            border.Loaded += Border_Loaded;

            if (icon == null && name == null)
            {
                border.HorizontalAlignment = HorizontalAlignment.Right;
                border.Margin = new Thickness(0, messageBlockTop, messageBlockSide, 0);
                border.Background = Brushes.YellowGreen;
            }
            else
            {
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = icon;
                imageBrush.Stretch = Stretch.UniformToFill;

                Ellipse ellipse = new Ellipse();
                ellipse.Fill = imageBrush;
                ellipse.Width = 80;
                ellipse.Height = ellipse.Width;
                ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse.VerticalAlignment = VerticalAlignment.Top;
                ellipse.Margin = new Thickness(20, messageBlockTop - ellipse.Height / 2, 0, 0);
                grid.Children.Add(ellipse);

                border.HorizontalAlignment = HorizontalAlignment.Left;
                border.Margin = new Thickness(ellipse.Margin.Left + ellipse.Width + messageBlockSide, messageBlockTop, 0, 0);
                border.Background = Brushes.White;

                TextBlock nameBlock = new TextBlock();
                nameBlock.Text = name;
                nameBlock.FontSize = 24;
                nameBlock.Foreground = Brushes.White;
                nameBlock.HorizontalAlignment = HorizontalAlignment.Left;
                nameBlock.VerticalAlignment = VerticalAlignment.Top;
                nameBlock.Margin = new Thickness(border.Margin.Left, ellipse.Margin.Top, 0, 0);
                grid.Children.Add(nameBlock);
            }

            grid.Children.Add(border);

            AudioFileReader audioFileReader = new AudioFileReader("send.wav");
            WaveOut waveOut = new WaveOut();
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;
            messageBlockTop += border.ActualHeight + messageBlockDistance;

            if (grid.Height < messageBlockTop)
            {
                grid.Children.Clear();
                messageBlockTop = messageBlockDistance;
                TextBlock textBlock = border.Child as TextBlock;
                border.Child = null;

                Border firstBorder = new Border();
                firstBorder.Child = textBlock;
                firstBorder.HorizontalAlignment = border.HorizontalAlignment;
                firstBorder.VerticalAlignment = border.VerticalAlignment;
                firstBorder.Margin = new Thickness(border.Margin.Left, messageBlockTop, border.Margin.Right, 0);
                firstBorder.Background = border.Background;
                firstBorder.CornerRadius = border.CornerRadius;
                firstBorder.Loaded += Border_Loaded;

                grid.Children.Add(firstBorder);
            }
        }
    }
}
