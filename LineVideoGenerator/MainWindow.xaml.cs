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
    public enum BackgroundType
    {
        Image,
        Animation,
        Default
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private double messageDistance = 10; // メッセージ同士の距離間隔
        private double messageTop; // メッセージの上の余白
        private double messageRight = 20; // メッセージの右の余白
        private double messageLeft = 100; // メッセージの左の余白
        private double iconSide = 10; // アイコンの横の余白
        private double nameTimeBottom = 4; // 名前・時刻の下の余白
        private double timeSide = 8; // 時刻の横の余白
        private int frameRate; // フレームレート
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        private List<Bitmap> messageBitmapList = new List<Bitmap>();
        private string tempPath = "temp";
        public string backgroundPath = "background.png";
        public BackgroundType backgroundType = BackgroundType.Default;
        public AudioFileReader bgm;
        public AudioFileReader soundEffect;
        public Data data = new Data();

        public MainWindow()
        {
            InitializeComponent();
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
            SettingsWindow backgroundWindow = new SettingsWindow();
            backgroundWindow.Owner = this;

            if (backgroundType != BackgroundType.Default) backgroundWindow.resetBackgroundButton.IsEnabled = true;
            if (bgm != null) backgroundWindow.resetBGMButton.IsEnabled = true;

            backgroundWindow.Show();
            backgroundButton.IsEnabled = false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            bool clickStopButton = false;
            void StopButton_Click(object sender2, RoutedEventArgs e2) => clickStopButton = true;

            // 編集画面と設定画面を閉じる
            Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.GetType() == typeof(EditWindow))?.Close();
            Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.GetType() == typeof(SettingsWindow))?.Close();

            editButton.IsEnabled = false;
            backgroundButton.IsEnabled = false;
            playButton.Content = "停止";
            playButton.Click -= PlayButton_Click;
            playButton.Click += StopButton_Click;
            saveButton.IsEnabled = false;

            // gridからすべての要素を削除
            messageGrid.Children.Clear();
            // gridの余白を初期化
            messageGrid.Margin = new Thickness(0);
            // メッセージの上の余白を初期化
            messageTop = groupBar.ActualHeight + messageDistance;

            foreach (var messageBitmap in messageBitmapList)
            {
                messageBitmap.Dispose();
            }
            messageBitmapList.Clear();

            // BGMを再生
            WaveOut waveOut = new WaveOut();
            bool loop = true;
            if (bgm != null)
            {
                waveOut.Init(bgm);
                waveOut.Play();
                waveOut.PlaybackStopped += (sender2, e2) =>
                {
                    bgm.Position = 0;
                    if(loop) waveOut.Play();
                };
            }

            // フレームレートを設定
            if (backgroundType == BackgroundType.Animation)
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
                    messageTimeSpan = TimeSpan.FromSeconds(data.videoTotalTime - data.messageCollection[i].Time);
                }
                else
                {
                    messageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i + 1].Time - data.messageCollection[i].Time);
                }

                if (i == 0)
                {
                    TimeSpan firstMessageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i].Time);
                    await Task.Delay(firstMessageTimeSpan);
                    AddMessageBitmap(firstMessageTimeSpan);
                }

                SendMessage(data.messageCollection[i]);
                await Task.Delay(messageTimeSpan);
                AddMessageBitmap(messageTimeSpan);

                // 停止
                if (clickStopButton) break;
            }

            // BGMを停止
            if (bgm != null)
            {
                loop = false;
                waveOut.Stop();
            }

            editButton.IsEnabled = true;
            backgroundButton.IsEnabled = true;
            playButton.Content = "再生";
            playButton.Click -= StopButton_Click;
            playButton.Click += PlayButton_Click;
            if (!clickStopButton) saveButton.IsEnabled = true;
        }

        private void SendMessage(Message message)
        {
            TextBlock messageBlock = new TextBlock();
            messageBlock.Text = message.Text;
            messageBlock.FontSize = 30;
            messageBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.MaxWidth = messageGrid.ActualWidth / 2;
            messageBlock.Margin = new Thickness(20, 10, 20, 10);

            Border messageBorder = new Border();
            messageBorder.Child = messageBlock;
            messageBorder.VerticalAlignment = VerticalAlignment.Top;
            messageBorder.CornerRadius = new CornerRadius(20);

            Image bubble = new Image();
            bubble.VerticalAlignment = VerticalAlignment.Top;
            bubble.Width = 20;

            TextBlock timeBlock = new TextBlock();
            timeBlock.Text = TimeSpan.FromSeconds(data.messageStartTime + message.Time).ToString(@"h\:mm");
            timeBlock.Foreground = Brushes.White;

            int messageIndex = data.messageCollection.IndexOf(message);
            bool personChanged = messageIndex == 0 || data.messageCollection[messageIndex - 1].person.id != message.person.id;

            if (message.person.id == 0)
            {
                if (personChanged)
                {
                    bubble.Source = new BitmapImage(new Uri("green bubble.png", UriKind.Relative));
                    bubble.HorizontalAlignment = HorizontalAlignment.Right;
                    bubble.Margin = new Thickness(0, messageTop, messageRight - bubble.Width / 2, 0);
                    messageGrid.Children.Add(bubble);
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                messageBorder.Margin = new Thickness(0, messageTop, messageRight, 0);
                messageBorder.Background = new SolidColorBrush(Color.FromArgb(255, 112, 222, 82));

                double timeBlockTop = messageBorder.Margin.Top + GetTextBlockHeight(messageBorder) - GetTextBlockHeight(timeBlock) - nameTimeBottom;
                double timeBlockRight = messageRight + GetTextBlockWidth(messageBorder) + timeSide;
                timeBlock.HorizontalAlignment = HorizontalAlignment.Right;
                timeBlock.Margin = new Thickness(0, timeBlockTop, timeBlockRight, 0);
            }
            else
            {
                if (personChanged)
                {
                    ImageBrush iconBrush = new ImageBrush();
                    iconBrush.ImageSource = message.person.Icon;
                    iconBrush.Stretch = Stretch.UniformToFill;

                    Ellipse iconEllipse = new Ellipse();
                    iconEllipse.Fill = iconBrush;
                    iconEllipse.Width = messageLeft - iconSide * 2 - bubble.Width / 2;
                    iconEllipse.Height = iconEllipse.Width;
                    iconEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                    iconEllipse.VerticalAlignment = VerticalAlignment.Top;
                    iconEllipse.Margin = new Thickness(iconSide, messageTop, 0, 0);
                    messageGrid.Children.Add(iconEllipse);

                    TextBlock nameBlock = new TextBlock();
                    nameBlock.Text = message.person.Name;
                    nameBlock.FontSize = 20;
                    nameBlock.Foreground = Brushes.White;
                    nameBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    nameBlock.VerticalAlignment = VerticalAlignment.Top;
                    nameBlock.Margin = new Thickness(messageLeft, messageTop, 0, 0);
                    messageGrid.Children.Add(nameBlock);

                    messageTop += GetTextBlockHeight(nameBlock) + nameTimeBottom;

                    bubble.Source = new BitmapImage(new Uri("white bubble.png", UriKind.Relative));
                    bubble.HorizontalAlignment = HorizontalAlignment.Left;
                    bubble.Margin = new Thickness(messageLeft - bubble.Width / 2, messageTop, 0, 0);
                    messageGrid.Children.Add(bubble);
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
                messageBorder.Margin = new Thickness(messageLeft, messageTop, 0, 0);
                messageBorder.Background = Brushes.White;

                double timeBlockTop = messageBorder.Margin.Top + GetTextBlockHeight(messageBorder) - GetTextBlockHeight(timeBlock) - nameTimeBottom;
                double timeBlockLeft = messageLeft + GetTextBlockWidth(messageBorder) + timeSide;
                timeBlock.HorizontalAlignment = HorizontalAlignment.Left;
                timeBlock.Margin = new Thickness(timeBlockLeft, timeBlockTop, 0, 0);
            }

            messageGrid.Children.Add(messageBorder);
            messageGrid.Children.Add(timeBlock);

            messageTop += GetTextBlockHeight(messageBorder) + messageDistance;

            double inputBarTop = messageGrid.ActualHeight - inputBar.ActualHeight;
            if (inputBarTop < messageTop)
            {
                double overHeight = messageTop - inputBarTop;
                messageGrid.Margin = new Thickness(messageGrid.Margin.Left, messageGrid.Margin.Top - overHeight, messageGrid.Margin.Right, messageGrid.Margin.Bottom);
            }

            // 効果音を再生
            if (soundEffect != null)
            {
                WaveOut waveOut = new WaveOut();
                waveOut.Init(soundEffect);
                waveOut.Play();
                waveOut.PlaybackStopped += (sender, e) => { soundEffect.Position = 0; };
            }

            // 音声を再生
            message.PlayVoice();
        }

        private double GetTextBlockWidth(FrameworkElement element)
        {
            // テキストブロックの幅を取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            return element.ActualWidth;
        }

        private double GetTextBlockHeight(FrameworkElement element)
        {
            // テキストブロックの高さを取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            return element.ActualHeight;
        }

        private void AddMessageBitmap(TimeSpan messageTimeSpan)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(screenGrid);
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            string messagePath = Path.Combine(tempPath, Guid.NewGuid() + ".png");
            using (var fileStream = File.Create(messagePath))
            {
                pngBitmapEncoder.Save(fileStream);
            }

            Bitmap messageBitmap = new Bitmap(messagePath);
            for (int j = 0; j < messageTimeSpan.TotalSeconds * frameRate; j++)
            {
                messageBitmapList.Add(messageBitmap);
            }
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
                    if (backgroundType == BackgroundType.Animation) SaveAnimationBackground(saveFileDialog.FileName);
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
            List<AudioFileReader> tempAudioList = new List<AudioFileReader>();
            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();

            // BGM
            if (bgm != null)
            {
                List<ISampleProvider> bgmList = new List<ISampleProvider>();

                for (int i = 0; i < (int)(data.videoTotalTime / bgm.TotalTime.TotalSeconds); i++)
                {
                    string bgmPath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                    WaveFileWriter.CreateWaveFile(bgmPath, bgm);
                    AudioFileReader bgmAudio = new AudioFileReader(bgmPath);
                    tempAudioList.Add(bgmAudio);
                    bgmList.Add(bgmAudio);
                    bgm.Position = 0;
                }

                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(bgm);
                offsetSampleProvider.Take = TimeSpan.FromSeconds(data.videoTotalTime % bgm.TotalTime.TotalSeconds);
                bgmList.Add(offsetSampleProvider);

                ConcatenatingSampleProvider concatenatingSampleProvider = new ConcatenatingSampleProvider(bgmList);
                sampleProviderList.Add(concatenatingSampleProvider);
            }

            // 効果音
            if (soundEffect != null)
            {
                foreach (var message in data.messageCollection)
                {
                    string soundEffectPath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                    WaveFileWriter.CreateWaveFile(soundEffectPath, soundEffect);
                    AudioFileReader soundEffectAudio = new AudioFileReader(soundEffectPath);
                    tempAudioList.Add(soundEffectAudio);
                    OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(soundEffectAudio);
                    offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                    sampleProviderList.Add(offsetSampleProvider);
                    soundEffect.Position = 0;
                }
            }

            // 音声
            foreach (var message in data.messageCollection.Where(m => m.IsSetVoice))
            {
                // 音声をWAV形式に変換
                string voicePath = Path.Combine(tempPath, Guid.NewGuid() + message.voicePathExt);
                File.WriteAllBytes(voicePath, message.voice);
                MediaFoundationReader mediaFoundationReader = new MediaFoundationReader(voicePath);
                string wavePath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                WaveFileWriter.CreateWaveFile(wavePath, new MediaFoundationResampler(mediaFoundationReader, new WaveFormat()));
                mediaFoundationReader.Dispose();

                AudioFileReader voiceAudio = new AudioFileReader(wavePath);
                tempAudioList.Add(voiceAudio);
                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(voiceAudio);
                offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                sampleProviderList.Add(offsetSampleProvider);
            }

            if (sampleProviderList.Count > 0)
            {
                // BGM・効果音・音声を合成
                string videoAudioPath = Path.Combine(tempPath, Guid.NewGuid() + ".wav");
                MixingSampleProvider mixingSampleProvider = new MixingSampleProvider(sampleProviderList);
                WaveFileWriter.CreateWaveFile16(videoAudioPath, mixingSampleProvider);

                foreach (var tempAudio in tempAudioList)
                {
                    tempAudio.Dispose();
                }

                //　動画にBGM・効果音・音声を挿入
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
