using Accord.Video.FFMPEG;
using AviFile;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using Accord;

namespace line
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<(BitmapImage icon, string name, string text)> messageList = new List<(BitmapImage icon, string name, string text)>();
        private const double messageBlockDistance = 50; // メッセージ同士の距離間隔
        private double messageBlockTop = messageBlockDistance;
        private double messageBlockSide = 30; // メッセージの横の余白
        private TimeSpan messageTimeSpan = TimeSpan.FromSeconds(1); // メッセージ同士の時間間隔
        private List<Bitmap> messageBitmapList = new List<Bitmap>();
        private List<Bitmap> backgroundBitmapList = new List<Bitmap>();
        private int frameRate; // フレームレート
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        private List<string> messagePathList = new List<string>(); // メッセージの画像の保存先
        private string backgroudPath = "snow.mp4"; // 背景のアニメーション
        private string soundEffectPath = "send.wav"; // サウンドエフェクト
        private string tempAudioPath = "temp.wav"; // 動画の音声の保存先
        private Ellipse tempEllipse;
        private TextBlock tempNameBlock;

        public MainWindow()
        {
            InitializeComponent();
            mediaElement.Source = new Uri(backgroudPath, UriKind.Relative);
            VideoFileReader videoFileReader = new VideoFileReader();
            videoFileReader.Open(backgroudPath);
            frameRate = (int)videoFileReader.FrameRate;
            videoFileReader.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = new EditWindow();
            editWindow.Owner = this;
            editWindow.Show();
            editButton.IsEnabled = false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            editButton.IsEnabled = false;
            playButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            grid.Children.Clear();
            foreach (var messageBitmap in messageBitmapList)
            {
                messageBitmap.Dispose();
            }
            messageBitmapList.Clear();
            messagePathList.Clear();
            messageBlockTop = messageBlockDistance;

            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();
            for (int i = 0; i < messageList.Count; i++)
            {
                AudioFileReader audioFileReader = new AudioFileReader(soundEffectPath);
                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(audioFileReader);
                if (i == 0)
                {
                    await Task.Delay(messageTimeSpan);
                    offsetSampleProvider.DelayBy = messageTimeSpan;
                    Bitmap bitmap = new Bitmap((int)grid.Width, (int)grid.Height);
                    for (int j = 0; j < messageTimeSpan.TotalSeconds * frameRate; j++)
                    {
                        messageBitmapList.Add(bitmap);
                    }
                }
                offsetSampleProvider.LeadOut = messageTimeSpan;
                sampleProviderList.Add(offsetSampleProvider);

                SendMessage(messageList[i].icon, messageList[i].name, messageList[i].text);
                await Task.Delay(audioFileReader.TotalTime + messageTimeSpan);

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)grid.Width, (int)grid.Height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(grid);
                PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                string messagePath = "message" + i + ".png";
                using (var fileStream = File.Create(messagePath))
                {
                    pngBitmapEncoder.Save(fileStream);
                    messagePathList.Add(messagePath);
                }

                Bitmap messageBitmap = new Bitmap(messagePath);
                for (int j = 0; j < (audioFileReader.TotalTime + messageTimeSpan).TotalSeconds * frameRate; j++)
                {
                    messageBitmapList.Add(messageBitmap);
                }
            }
            ConcatenatingSampleProvider concatenatingSampleProvider = new ConcatenatingSampleProvider(sampleProviderList);
            WaveFileWriter.CreateWaveFile16(tempAudioPath, concatenatingSampleProvider);

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
                VideoStream aviStream = null;
                VideoFileReader videoFileReader = new VideoFileReader();
                videoFileReader.Open(backgroudPath);
                for (int i = 0; i < messageBitmapList.Count; i++)
                {
                    Bitmap backgroundBitmap = videoFileReader.ReadVideoFrame();
                    Bitmap frameBitmap = new Bitmap(frameWidth, frameHeight);
                    Graphics graphics = Graphics.FromImage(frameBitmap);
                    graphics.DrawImage(backgroundBitmap, 0, 0, frameBitmap.Width, frameBitmap.Height);
                    graphics.DrawImage(messageBitmapList[i], 0, 0, frameBitmap.Width, frameBitmap.Height);

                    if (i == 0)
                    {
                        aviStream = aviManager.AddVideoStream(false, frameRate, frameBitmap);
                    } else
                    {
                        aviStream.AddFrame(frameBitmap);
                    }

                    if (i % videoFileReader.FrameCount == 0)
                    {
                        videoFileReader.Close();
                        videoFileReader.Open(backgroudPath);
                    }

                    backgroundBitmap.Dispose();
                    frameBitmap.Dispose();
                    graphics.Dispose();
                }
                videoFileReader.Close();
                aviManager.AddAudioStream(tempAudioPath, 0);
                aviManager.Close();
                MessageBox.Show("保存されました");
            }
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
                tempEllipse = ellipse;
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
                tempNameBlock = nameBlock;
                grid.Children.Add(nameBlock);
            }

            grid.Children.Add(border);

            AudioFileReader audioFileReader = new AudioFileReader(soundEffectPath);
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

                Border tempBorder = new Border();
                tempBorder.Child = textBlock;
                tempBorder.HorizontalAlignment = border.HorizontalAlignment;
                tempBorder.VerticalAlignment = border.VerticalAlignment;
                tempBorder.Margin = new Thickness(border.Margin.Left, messageBlockTop, border.Margin.Right, 0);
                tempBorder.Background = border.Background;
                tempBorder.CornerRadius = border.CornerRadius;
                tempBorder.Loaded += Border_Loaded;

                if (tempBorder.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    tempEllipse.Margin = new Thickness(20, messageBlockTop - tempEllipse.Height / 2, 0, 0);
                    grid.Children.Add(tempEllipse);
                    tempNameBlock.Margin = new Thickness(tempBorder.Margin.Left, tempEllipse.Margin.Top, 0, 0);
                    grid.Children.Add(tempNameBlock);
                }

                grid.Children.Add(tempBorder);
            }
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var messageBitmap in messageBitmapList)
            {
                messageBitmap.Dispose();
            }
            foreach (var messagePath in messagePathList)
            {
                File.Delete(messagePath);
            }
            File.Delete(tempAudioPath);
        }
    }
}
