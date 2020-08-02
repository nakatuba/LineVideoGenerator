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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using Brushes = System.Windows.Media.Brushes;
using Size = System.Windows.Size;

namespace LineVideoGenerator
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double messageBlockDistance = 10; // メッセージ同士の距離間隔
        private double messageBlockTop = messageBlockDistance; // メッセージの上の余白
        private double messageBlockRight = 20; // メッセージの右の余白
        private double messageBlockLeft = 100; // メッセージの右の余白
        private double iconEllipseLeft = 10; // アイコンの左の余白
        private TimeSpan messageTimeSpan = TimeSpan.FromSeconds(1); // メッセージ同士の時間間隔
        private List<Bitmap> messageBitmapList = new List<Bitmap>();
        public int frameRate; // フレームレート
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        private List<string> messagePathList = new List<string>(); // メッセージの画像の保存先
        public string backgroundPath = "background.png"; // 背景
        public bool isAnimated = false;
        private string soundEffectPath = "send.mp3"; // サウンドエフェクト
        private string tempAudioPath = "temp.wav"; // 動画の音声の保存先
        public Data data = new Data();

        public MainWindow()
        {
            InitializeComponent();

            // 背景（デフォルト）
            BitmapImage bitmapImage = new BitmapImage(new Uri(backgroundPath, UriKind.Relative));
            backgroundImage.Source = bitmapImage;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files|*.xml";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.OpenRead(openFileDialog.FileName))
                    {
                        data = (Data)se.Deserialize(fs);
                    }
                    playButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = new EditWindow();
            editWindow.Owner = this;
            editWindow.Show();
            editButton.IsEnabled = false;
        }

        private void BackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWindow backgroundWindow = new BackgroundWindow();
            backgroundWindow.Owner = this;
            backgroundWindow.Show();
            if (backgroundPath != "background.png") backgroundWindow.resetButton.IsEnabled = true;
            backgroundButton.IsEnabled = false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            editButton.IsEnabled = false;
            playButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            // gridからすべての要素を削除
            grid.Children.Clear();
            // gridの高さを初期化
            grid.Height = 540;
            // gridからアニメーションを削除（https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/graphics-multimedia/how-to-set-a-property-after-animating-it-with-a-storyboard）
            grid.BeginAnimation(MarginProperty, null);
            // gridの余白を初期化
            grid.Margin = new Thickness(0);
            foreach (var messageBitmap in messageBitmapList)
            {
                messageBitmap.Dispose();
            }
            messageBitmapList.Clear();
            messagePathList.Clear();
            messageBlockTop = messageBlockDistance;

            // フレームレートを設定
            if(isAnimated)
            {
                VideoFileReader videoFileReader = new VideoFileReader();
                videoFileReader.Open(backgroundPath);
                frameRate = (int)videoFileReader.FrameRate;
                videoFileReader.Close();
            }
            else
            {
                frameRate = 25;
            }

            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();
            for (int i = 0; i < data.messageList.Count; i++)
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

                SendMessage(data.messageList[i]);
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

        public void SendMessage(Message message)
        {
            TextBlock messageBlock = new TextBlock();
            messageBlock.Text = message.text;
            messageBlock.FontSize = 30;
            messageBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.MaxWidth = grid.Width / 2;
            messageBlock.Margin = new Thickness(20, 10, 20, 10);

            Border messageBorder = new Border();
            messageBorder.Child = messageBlock;
            messageBorder.VerticalAlignment = VerticalAlignment.Top;
            messageBorder.CornerRadius = new CornerRadius(20);

            if (message.personID == 1)
            {
                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                messageBorder.Margin = new Thickness(0, messageBlockTop, messageBlockRight, 0);
                messageBorder.Background = Brushes.YellowGreen;
            }
            else
            {
                int messageIndex = data.messageList.IndexOf(message);
                if (messageIndex == 0 || data.messageList[messageIndex - 1].personID != message.personID)
                {
                    ImageBrush iconBrush = new ImageBrush();
                    iconBrush.ImageSource = message.icon;
                    iconBrush.Stretch = Stretch.UniformToFill;

                    Ellipse iconEllipse = new Ellipse();
                    iconEllipse.Fill = iconBrush;
                    iconEllipse.Width = messageBlockLeft - messageBlockRight - iconEllipseLeft;
                    iconEllipse.Height = iconEllipse.Width;
                    iconEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                    iconEllipse.VerticalAlignment = VerticalAlignment.Top;
                    iconEllipse.Margin = new Thickness(iconEllipseLeft, messageBlockTop, 0, 0);
                    grid.Children.Add(iconEllipse);

                    TextBlock nameBlock = new TextBlock();
                    nameBlock.Text = message.name;
                    nameBlock.FontSize = 20;
                    nameBlock.Foreground = Brushes.White;
                    nameBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    nameBlock.VerticalAlignment = VerticalAlignment.Top;
                    nameBlock.Margin = new Thickness(messageBlockLeft, messageBlockTop, 0, 0);
                    grid.Children.Add(nameBlock);

                    // nameBlockの高さを取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
                    nameBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    nameBlock.Arrange(new Rect(nameBlock.DesiredSize));
                    messageBlockTop += nameBlock.ActualHeight + messageBlockDistance;
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
                messageBorder.Margin = new Thickness(messageBlockLeft, messageBlockTop, 0, 0);
                messageBorder.Background = Brushes.White;
            }

            grid.Children.Add(messageBorder);

            // borderの高さを取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
            messageBorder.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            messageBorder.Arrange(new Rect(messageBorder.DesiredSize));
            messageBlockTop += messageBorder.ActualHeight + messageBlockDistance;

            if (grid.Height < messageBlockTop)
            {
                double overHeight = messageBlockTop - grid.Height;
                grid.Height += overHeight;

                // アニメーションを作成
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.From = grid.Margin;
                thicknessAnimation.To = new Thickness(grid.Margin.Left, grid.Margin.Top - overHeight, grid.Margin.Right, grid.Margin.Bottom);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));

                // アニメーションを再生
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(thicknessAnimation);
                Storyboard.SetTarget(thicknessAnimation, grid);
                Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(MarginProperty));
                storyboard.Begin();
            }

            // サウンドエフェクトを再生
            AudioFileReader audioFileReader = new AudioFileReader(soundEffectPath);
            WaveOut waveOut = new WaveOut();
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".avi";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (isAnimated) SaveAnimationBackground(saveFileDialog.FileName);
                    else SaveImageBackground(saveFileDialog.FileName);
                    MessageBox.Show("保存されました");
                }
                catch (Exception)
                {
                    MessageBox.Show("保存できませんでした");
                }
            }
            */

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".xml";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.Create(saveFileDialog.FileName))
                    {
                        se.Serialize(fs, data);
                    }
                    MessageBox.Show("保存されました");
                }
                catch (Exception)
                {
                    MessageBox.Show("保存できませんでした");
                }
            }
        }

        private void SaveImageBackground(string fileName)
        {
            VideoFileWriter videoFileWriter = new VideoFileWriter();
            videoFileWriter.Open(fileName, frameWidth, frameHeight, frameRate);
            for (int i = 0; i < messageBitmapList.Count; i++)
            {
                Bitmap backgroundBitmap = new Bitmap(backgroundPath);
                Bitmap frameBitmap = new Bitmap(frameWidth, frameHeight);
                Graphics graphics = Graphics.FromImage(frameBitmap);
                graphics.DrawImage(backgroundBitmap, 0, 0, frameBitmap.Width, frameBitmap.Height);
                graphics.DrawImage(messageBitmapList[i], 0, 0, frameBitmap.Width, frameBitmap.Height);

                videoFileWriter.WriteVideoFrame(frameBitmap);

                backgroundBitmap.Dispose();
                frameBitmap.Dispose();
                graphics.Dispose();
            }
            videoFileWriter.Close();

            AviManager aviManager = new AviManager(fileName, true);
            aviManager.AddAudioStream(tempAudioPath, 0);
            aviManager.Close();
        }

        private void SaveAnimationBackground(string fileName)
        {
            VideoFileReader videoFileReader = new VideoFileReader();
            videoFileReader.Open(backgroundPath);
            VideoFileWriter videoFileWriter = new VideoFileWriter();
            videoFileWriter.Open(fileName, frameWidth, frameHeight, frameRate);
            for (int i = 0; i < messageBitmapList.Count; i++)
            {
                Bitmap backgroundBitmap = videoFileReader.ReadVideoFrame();
                Bitmap frameBitmap = new Bitmap(frameWidth, frameHeight);
                Graphics graphics = Graphics.FromImage(frameBitmap);
                graphics.DrawImage(backgroundBitmap, 0, 0, frameBitmap.Width, frameBitmap.Height);
                graphics.DrawImage(messageBitmapList[i], 0, 0, frameBitmap.Width, frameBitmap.Height);

                videoFileWriter.WriteVideoFrame(frameBitmap);

                if (i % videoFileReader.FrameCount == 0)
                {
                    videoFileReader.Close();
                    videoFileReader.Open(backgroundPath);
                }

                backgroundBitmap.Dispose();
                frameBitmap.Dispose();
                graphics.Dispose();
            }
            videoFileReader.Close();
            videoFileWriter.Close();

            AviManager aviManager = new AviManager(fileName, true);
            aviManager.AddAudioStream(tempAudioPath, 0);
            aviManager.Close();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
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
