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
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
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
        private double messageBlockLeft = 100; // メッセージの左の余白
        private double iconEllipseSide = 10; // アイコンの横の余白
        private int gridOriginalHeight = 540; // gridの元の高さ
        private int frameRate; // フレームレート
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        public Data data = new Data();
        private List<Bitmap> messageBitmapList = new List<Bitmap>();
        private string tempPath = "temp";
        public string backgroundPath = "background.png";
        public bool isAnimated = false;
        private string soundEffectPath = "send.mp3";

        public MainWindow()
        {
            InitializeComponent();
            SetMessageCollectionChanged();
            Directory.CreateDirectory(tempPath);
        }

        /// <summary>
        /// メッセージの追加・削除後に保存ボタンを無効化し、メッセージがあれば再生ボタンを有効化するよう設定
        /// </summary>
        public void SetMessageCollectionChanged()
        {
            data.messageCollection.CollectionChanged += (sender, e) =>
            {
                saveButton.IsEnabled = false;
                playButton.IsEnabled = data.messageCollection.Count > 0;
            };
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
            bool clickStopButton = false;
            void StopButton_Click(object sender2, RoutedEventArgs e2) => clickStopButton = true;

            // 編集画面を閉じる
            Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.GetType() == typeof(EditWindow))?.Close();

            editButton.IsEnabled = false;
            playButton.Content = "停止";
            playButton.Click -= PlayButton_Click;
            playButton.Click += StopButton_Click;
            saveButton.IsEnabled = false;

            // gridからすべての要素を削除
            grid.Children.Clear();
            // gridの高さを初期化
            grid.Height = gridOriginalHeight;
            // gridからアニメーションを削除（https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/graphics-multimedia/how-to-set-a-property-after-animating-it-with-a-storyboard）
            grid.BeginAnimation(MarginProperty, null);
            // gridの余白を初期化
            grid.Margin = new Thickness(0);

            messageBlockTop = messageBlockDistance;

            foreach (var messageBitmap in messageBitmapList)
            {
                messageBitmap.Dispose();
            }
            messageBitmapList.Clear();

            // フレームレートを設定
            if (isAnimated)
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

            for (int i = 0; i < data.messageCollection.Count; i++)
            {
                // 次のメッセージとの時間差
                TimeSpan messageTimeSpan;
                if (i == data.messageCollection.Count - 1)
                {
                    messageTimeSpan = TimeSpan.FromSeconds(1); // videoTotalTime - data.messageList[i].time
                }
                else
                {
                    messageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i + 1].Time - data.messageCollection[i].Time);
                }

                if (i == 0)
                {
                    TimeSpan firstMessageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i].Time);
                    await Task.Delay(firstMessageTimeSpan);
                    Bitmap firstMessageBitmap = new Bitmap((int)grid.Width, (int)grid.Height);
                    for (int j = 0; j < firstMessageTimeSpan.TotalSeconds * frameRate; j++)
                    {
                        messageBitmapList.Add(firstMessageBitmap);
                    }
                }

                SendMessage(data.messageCollection[i]);
                await Task.Delay(messageTimeSpan);

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)grid.Width, gridOriginalHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(grid);
                PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                string messagePath = Path.Combine(tempPath, Guid.NewGuid() + ".png");
                // messagePathList.Add(messagePath);
                using (var fileStream = File.Create(messagePath))
                {
                    pngBitmapEncoder.Save(fileStream);
                }

                Bitmap messageBitmap = new Bitmap(messagePath);
                for (int j = 0; j < messageTimeSpan.TotalSeconds * frameRate; j++)
                {
                    messageBitmapList.Add(messageBitmap);
                }

                // 停止
                if (clickStopButton) break;
            }

            editButton.IsEnabled = true;
            playButton.Content = "再生";
            playButton.Click -= StopButton_Click;
            playButton.Click += PlayButton_Click;
            if (!clickStopButton) saveButton.IsEnabled = true;
        }

        public void SendMessage(Message message)
        {
            TextBlock messageBlock = new TextBlock();
            messageBlock.Text = message.Text;
            messageBlock.FontSize = 30;
            messageBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.MaxWidth = grid.Width / 2;
            messageBlock.Margin = new Thickness(20, 10, 20, 10);

            Border messageBorder = new Border();
            messageBorder.Child = messageBlock;
            messageBorder.VerticalAlignment = VerticalAlignment.Top;
            messageBorder.CornerRadius = new CornerRadius(20);

            // 吹き出し
            Image bubble = new Image();
            bubble.VerticalAlignment = VerticalAlignment.Top;
            bubble.Width = 20;

            if (message.person.id == 0)
            {
                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                messageBorder.Margin = new Thickness(0, messageBlockTop, messageBlockRight, 0);
                messageBorder.Background = new SolidColorBrush(Color.FromArgb(255, 112, 222, 82));

                bubble.Source = new BitmapImage(new Uri("green bubble.png", UriKind.Relative));
                bubble.HorizontalAlignment = HorizontalAlignment.Right;
                bubble.Margin = new Thickness(0, messageBlockTop, messageBlockRight - bubble.Width / 2, 0);
                grid.Children.Add(bubble);
            }
            else
            {
                int messageIndex = data.messageCollection.IndexOf(message);
                if (messageIndex == 0 || data.messageCollection[messageIndex - 1].person.id != message.person.id)
                {
                    ImageBrush iconBrush = new ImageBrush();
                    iconBrush.ImageSource = message.person.Icon;
                    iconBrush.Stretch = Stretch.UniformToFill;

                    Ellipse iconEllipse = new Ellipse();
                    iconEllipse.Fill = iconBrush;
                    iconEllipse.Width = messageBlockLeft - iconEllipseSide * 2 - bubble.Width / 2;
                    iconEllipse.Height = iconEllipse.Width;
                    iconEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                    iconEllipse.VerticalAlignment = VerticalAlignment.Top;
                    iconEllipse.Margin = new Thickness(iconEllipseSide, messageBlockTop, 0, 0);
                    grid.Children.Add(iconEllipse);

                    TextBlock nameBlock = new TextBlock();
                    nameBlock.Text = message.person.Name;
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

                    bubble.Source = new BitmapImage(new Uri("white bubble.png", UriKind.Relative));
                    bubble.HorizontalAlignment = HorizontalAlignment.Left;
                    bubble.Margin = new Thickness(messageBlockLeft - bubble.Width / 2, messageBlockTop, 0, 0);
                    grid.Children.Add(bubble);
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
                grid.Margin = new Thickness(grid.Margin.Left, grid.Margin.Top - overHeight, grid.Margin.Right, grid.Margin.Bottom);
            }

            // サウンドエフェクトを再生
            AudioFileReader audioFileReader = new AudioFileReader(soundEffectPath);
            WaveOut waveOut = new WaveOut();
            waveOut.Init(audioFileReader);
            waveOut.Play();

            // 音声を再生
            message.PlayVoice();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".avi";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 動画を保存
                    if (isAnimated) SaveAnimationBackground(saveFileDialog.FileName);
                    else SaveImageBackground(saveFileDialog.FileName);
                    AddVideoAudio(saveFileDialog.FileName);

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
        }

        private void AddVideoAudio(string fileName)
        {
            List<AudioFileReader> audioFileReaderList = new List<AudioFileReader>();
            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();
            foreach (var message in data.messageCollection.Where(m => m.IsSetVoice))
            {
                // 音声をWAV形式に変換
                string voicePath = Path.Combine(tempPath, Guid.NewGuid() + message.voicePathExt);
                File.WriteAllBytes(voicePath, message.Voice);
                MediaFoundationReader mediaFoundationReader = new MediaFoundationReader(voicePath);
                string wavePath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                WaveFileWriter.CreateWaveFile(wavePath, new MediaFoundationResampler(mediaFoundationReader, new WaveFormat()));
                mediaFoundationReader.Dispose();

                AudioFileReader audioFileReader = new AudioFileReader(wavePath);
                audioFileReaderList.Add(audioFileReader);
                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(audioFileReader);
                offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                sampleProviderList.Add(offsetSampleProvider);
            }

            if (sampleProviderList.Count > 0)
            {
                // 音声を合成
                string videoAudioPath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                MixingSampleProvider mixingSampleProvider = new MixingSampleProvider(sampleProviderList);
                WaveFileWriter.CreateWaveFile16(videoAudioPath, mixingSampleProvider);

                foreach (var audioFileReader in audioFileReaderList)
                {
                    audioFileReader.Dispose();
                }

                //　動画に音声を挿入
                AviManager aviManager = new AviManager(fileName, true);
                aviManager.AddAudioStream(videoAudioPath, 0);
                aviManager.Close();
            }
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

            Directory.Delete(tempPath, true);
        }
    }
}
