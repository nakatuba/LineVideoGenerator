using Accord.Video.FFMPEG;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;

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
        private double timeBottom = 4; // 時刻の下の余白
        private double timeSide = 8; // 時刻の横の余白
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        private int bitRate = 8000000; // ビットレート
        public static string tempDirectory = "temp";
        public BackgroundType backgroundType = BackgroundType.Default;
        public Data data = new Data();

        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(tempDirectory);
        }

        /// <summary>
        /// メッセージの追加・削除後にメッセージがあれば再生ボタンと保存ボタンを有効化し、メッセージがなければ無効化するよう設定
        /// </summary>
        public void SetMessageCollectionChanged()
        {
            data.messageCollection.CollectionChanged += (sender, e) =>
            {
                playButton.IsEnabled = data.messageCollection.Count > 0;
                saveButton.IsEnabled = data.messageCollection.Count > 0;
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
            if (backgroundType != BackgroundType.Default) backgroundWindow.resetButton.IsEnabled = true;
            backgroundWindow.Show();
            backgroundButton.IsEnabled = false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            bool clickStopButton = false;
            void StopButton_Click(object sender2, RoutedEventArgs e2) => clickStopButton = true;

            // 再生ボタンを停止ボタンに変更
            playButton.Content = "停止";
            playButton.Click -= PlayButton_Click;
            playButton.Click += StopButton_Click;

            // メインウィンドウ以外のウィンドウを閉じる
            foreach (var window in Application.Current.Windows.Cast<Window>().Where(w => w != this))
            {
                window.Close();
            }

            // 再生ボタン以外のボタンを無効化
            Grid grid = playButton.Parent as Grid;
            foreach (var button in grid.Children.Cast<Button>().Where(b => b != playButton))
            {
                button.IsEnabled = false;
            }

            // メッセージグリッドからすべての要素を削除
            messageGrid.Children.Clear();
            // メッセージグリッドの余白を初期化
            messageGrid.Margin = new Thickness(0);
            // メッセージの上の余白を初期化
            messageTop = groupBar.ActualHeight + messageDistance;

            // BGMを再生
            WaveOut waveOut = new WaveOut();
            bool loop = true;
            if (data.bgm != null)
            {
                string bgmPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(bgmPath, data.bgm);
                AudioFileReader bgmAudio = new AudioFileReader(bgmPath);

                waveOut.Init(bgmAudio);
                waveOut.Play();
                waveOut.PlaybackStopped += (sender2, e2) =>
                {
                    if (loop)
                    {
                        bgmAudio.Position = 0;
                        if (loop) waveOut.Play();
                    }
                    else
                    {
                        bgmAudio.Dispose();
                    }
                };
            }

            for (int i = 0; i < data.messageCollection.Count; i++)
            {
                if (i == 0)
                {
                    TimeSpan firstMessageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i].Time);
                    await Task.Delay(firstMessageTimeSpan);
                }

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

                SendMessage(data.messageCollection[i]);

                // 効果音を再生
                Global.PlayByteArray(data.soundEffect);

                // 音声を再生
                Global.PlayByteArray(data.messageCollection[i].voice);

                await Task.Delay(messageTimeSpan);

                // 停止
                if (clickStopButton) break;
            }

            // BGMを停止
            if (data.bgm != null)
            {
                loop = false;
                waveOut.Stop();
            }

            // 再生ボタン以外のボタンを有効化
            foreach (var button in grid.Children.Cast<Button>().Where(b => b != playButton))
            {
                button.IsEnabled = true;
            }

            // 停止ボタンを再生ボタンに戻す
            playButton.Content = "再生";
            playButton.Click -= StopButton_Click;
            playButton.Click += PlayButton_Click;
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

                double timeBlockTop = messageBorder.Margin.Top + Global.GetHeight(messageBorder) - Global.GetHeight(timeBlock) - timeBottom;
                double timeBlockRight = messageRight + Global.GetWidth(messageBorder) + timeSide;
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

                    messageTop += Global.GetHeight(nameBlock) + 4;

                    bubble.Source = new BitmapImage(new Uri("white bubble.png", UriKind.Relative));
                    bubble.HorizontalAlignment = HorizontalAlignment.Left;
                    bubble.Margin = new Thickness(messageLeft - bubble.Width / 2, messageTop, 0, 0);
                    messageGrid.Children.Add(bubble);
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
                messageBorder.Margin = new Thickness(messageLeft, messageTop, 0, 0);
                messageBorder.Background = Brushes.White;

                double timeBlockTop = messageBorder.Margin.Top + Global.GetHeight(messageBorder) - Global.GetHeight(timeBlock) - timeBottom;
                double timeBlockLeft = messageLeft + Global.GetWidth(messageBorder) + timeSide;
                timeBlock.HorizontalAlignment = HorizontalAlignment.Left;
                timeBlock.Margin = new Thickness(timeBlockLeft, timeBlockTop, 0, 0);
            }

            messageGrid.Children.Add(messageBorder);
            messageGrid.Children.Add(timeBlock);

            messageTop += Global.GetHeight(messageBorder) + messageDistance;

            double inputBarTop = messageGrid.ActualHeight - inputBar.ActualHeight;
            if (inputBarTop < messageTop)
            {
                double overHeight = messageTop - inputBarTop;
                messageGrid.Margin = new Thickness(messageGrid.Margin.Left, messageGrid.Margin.Top - overHeight, messageGrid.Margin.Right, messageGrid.Margin.Bottom);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "avi|*.avi";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    progressRing.IsActive = true;

                    // メインウィンドウ以外のウィンドウを閉じる
                    foreach (var window in Application.Current.Windows.Cast<Window>().Where(w => w != this))
                    {
                        window.Close();
                    }

                    // すべてのボタンを無効化
                    Grid grid = saveButton.Parent as Grid;
                    foreach (var button in grid.Children.Cast<Button>())
                    {
                        button.IsEnabled = false;
                    }

                    // メッセージグリッドからすべての要素を削除
                    messageGrid.Children.Clear();
                    // メッセージグリッドの余白を初期化
                    messageGrid.Margin = new Thickness(0);
                    // メッセージの上の余白を初期化
                    messageTop = groupBar.ActualHeight + messageDistance;

                    // 動画を作成
                    string videoPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".avi");
                    using (var writer = new VideoFileWriter())
                    {
                        writer.Open(videoPath, (int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight, 25, VideoCodec.Default, bitRate);
                        using (var messageBitmap = GetMessageBitmap())
                        {
                            writer.WriteVideoFrame(messageBitmap);
                        }

                        foreach (var message in data.messageCollection)
                        {
                            SendMessage(message);
                            await Task.Delay(100);
                            using (var messageBitmap = GetMessageBitmap())
                            {
                                writer.WriteVideoFrame(messageBitmap, TimeSpan.FromSeconds(message.Time));
                            }
                        }

                        using (var messageBitamap = GetMessageBitmap())
                        {
                            writer.WriteVideoFrame(messageBitamap, TimeSpan.FromSeconds(data.videoTotalTime));
                        }
                    }

                    if (backgroundType == BackgroundType.Animation)
                    {
                        string animationPath = mediaElement.Source.OriginalString;
                        await Task.Run(() => AddAnimationBackground(animationPath, ref videoPath));
                    }

                    await Task.Run(() => AddVideoAudio(videoPath, saveFileDialog.FileName));

                    // すべてのボタンを有効化
                    foreach (var button in grid.Children.Cast<Button>())
                    {
                        button.IsEnabled = true;
                    }

                    progressRing.IsActive = false;
                    MessageBox.Show("保存されました");
                }
                catch (Exception)
                {
                    MessageBox.Show("保存できませんでした");
                }
            }
        }

        private Bitmap GetMessageBitmap()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(screenGrid);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            string messagePath = Path.Combine(tempDirectory, Guid.NewGuid() + ".png");
            using (var fs = File.Create(messagePath))
            {
                encoder.Save(fs);
            }

            return new Bitmap(messagePath);
        }

        private void AddAnimationBackground(string animationPath, ref string videoPath)
        {
            // 背景動画を作成
            string loopPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".avi");
            using (var process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = $"-stream_loop -1 -i {animationPath} -b {bitRate} -t {data.videoTotalTime} -s {screenGrid.ActualWidth}x{screenGrid.ActualHeight} {loopPath}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }

            // 背景動画を合成（https://qiita.com/developer-kikikaikai/items/47a13cbcb6fdb535345a）
            string overlayPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".avi");
            using (var process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = $"-i {loopPath} -i {videoPath} -filter_complex \"[1:0]colorkey[output];[0:0][output]overlay\" -b {bitRate} {overlayPath}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }
            
            videoPath = overlayPath;
        }

        private void AddVideoAudio(string inputPath, string outputPath)
        {
            List<AudioFileReader> tempAudioList = new List<AudioFileReader>();
            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();

            // BGM
            if (data.bgm != null)
            {
                string bgmPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(bgmPath, data.bgm);

                AudioFileReader bgmAudio = new AudioFileReader(Global.GetLoopAudio(bgmPath, data.videoTotalTime));
                tempAudioList.Add(bgmAudio);

                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(bgmAudio);
                sampleProviderList.Add(offsetSampleProvider);
            }

            // 効果音
            if (data.soundEffect != null)
            {
                foreach (var message in data.messageCollection)
                {
                    string soundEffectPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                    File.WriteAllBytes(soundEffectPath, data.soundEffect);

                    AudioFileReader soundEffectAudio = new AudioFileReader(soundEffectPath);
                    tempAudioList.Add(soundEffectAudio);

                    OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(soundEffectAudio);
                    offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                    sampleProviderList.Add(offsetSampleProvider);
                }
            }

            // 音声
            foreach (var message in data.messageCollection.Where(m => m.IsSetVoice))
            {
                string voicePath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(voicePath, message.voice);

                AudioFileReader voiceAudio = new AudioFileReader(voicePath);
                tempAudioList.Add(voiceAudio);

                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(voiceAudio);
                offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                sampleProviderList.Add(offsetSampleProvider);
            }

            if (sampleProviderList.Count > 0)
            {
                // BGM・効果音・音声を合成
                string videoAudioPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                MixingSampleProvider mixingSampleProvider = new MixingSampleProvider(sampleProviderList);
                WaveFileWriter.CreateWaveFile16(videoAudioPath, mixingSampleProvider);

                foreach (var tempAudio in tempAudioList)
                {
                    tempAudio.Dispose();
                }

                // 動画に音声を挿入
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg.exe";
                    process.StartInfo.Arguments = $"-y -i {inputPath} -i {videoAudioPath} -b {bitRate} -s {frameWidth}x{frameHeight} {outputPath}";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.WaitForExit();
                }
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
