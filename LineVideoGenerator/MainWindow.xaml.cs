using Accord.Video.FFMPEG;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
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
        private double nameBottom = 4; // 名前の下の余白
        private double timeBottom = 4; // 時刻の下の余白
        private double timeSide = 8; // 時刻の横の余白
        private int frameRate = 25; // フレームレート
        private int frameWidth = 1920; // フレームの幅
        private int frameHeight = 1080; // フレームの高さ
        public static string tempDirectory = "temp";
        public BackgroundType backgroundType = BackgroundType.Default;
        public Data data = new Data();

        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(tempDirectory);
        }

        public void SetMessageCollectionCollectionChanged()
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
            WaveOut bgmWave = new WaveOut();
            bool bgmLoop = true;
            if (data.bgm != null)
            {
                string bgmPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(bgmPath, data.bgm);
                AudioFileReader bgmAudio = new AudioFileReader(bgmPath);

                bgmWave.Init(bgmAudio);
                bgmWave.Play();
                bgmWave.PlaybackStopped += (sender2, e2) =>
                {
                    if (bgmLoop)
                    {
                        bgmAudio.Position = 0;
                        if (bgmLoop) bgmWave.Play();
                    }
                    else
                    {
                        bgmAudio.Dispose();
                    }
                };
            }

            // 背景動画を再生
            if (backgroundType == BackgroundType.Animation)
            {
                mediaElement.Position = TimeSpan.Zero;
                mediaElement.Play();
            }

            for (int i = 0; i < data.messageCollection.First().Duration; i++)
            {
                if (clickStopButton) break;
                await Task.Delay(1000);
            }

            DateTime time = new DateTime();
            if (data.startTimeList.Any(s => s.Duration <= data.messageCollection.First().Duration))
            {
                time = data.startTimeList.OrderBy(s => s.Duration).Last(s => s.Duration <= data.messageCollection.First().Duration).Time;
            }

            for (int i = 0; i < data.messageCollection.Count; i++)
            {
                if (clickStopButton) break;

                if (i != 0 && data.startTimeList.Any(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration))
                {
                    time = data.startTimeList.OrderBy(s => s.Duration).Last(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration).Time;
                }

                DrawMessage(data.messageCollection[i], ref time);

                // 効果音を再生
                WaveOut soundEffectWave = new WaveOut();
                if (data.soundEffect != null)
                {
                    string path = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                    File.WriteAllBytes(path, data.soundEffect);
                    AudioFileReader soundEffectAudio = new AudioFileReader(path);

                    soundEffectWave.Init(soundEffectAudio);
                    soundEffectWave.Play();
                    soundEffectWave.PlaybackStopped += (sender2, e2) => soundEffectAudio.Dispose();
                }

                // 音声を再生
                WaveOut voideWave = new WaveOut();
                if (data.messageCollection[i].Voice != null)
                {
                    string path = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                    File.WriteAllBytes(path, data.messageCollection[i].Voice);
                    AudioFileReader voiceAudio = new AudioFileReader(path);

                    voideWave.Init(voiceAudio);
                    voideWave.Play();
                    voideWave.PlaybackStopped += (sender2, e2) => voiceAudio.Dispose();
                }

                // 次のメッセージとの時間差
                int durationInterval = (i == data.messageCollection.Count - 1)
                                       ? data.videoTotalTime - data.messageCollection[i].Duration
                                       : data.messageCollection[i + 1].Duration - data.messageCollection[i].Duration;
                for (int j = 0; j < durationInterval; j++)
                {
                    if (clickStopButton)
                    {
                        // 効果音を停止
                        soundEffectWave.Stop();
                        // 音声を停止
                        voideWave.Stop();
                        break;
                    }
                    await Task.Delay(1000);
                }
            }

            // 背景動画を停止
            if (backgroundType == BackgroundType.Animation)
            {
                mediaElement.Pause();
            }

            // BGMを停止
            if (data.bgm != null)
            {
                bgmLoop = false;
                bgmWave.Stop();
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

        private void DrawMessage(Message message, ref DateTime time)
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
            messageBorder.Background = new SolidColorBrush(message.Color);
            messageBorder.CornerRadius = new CornerRadius(20);

            Image bubble = new Image();
            bubble.VerticalAlignment = VerticalAlignment.Top;
            bubble.Width = 20;

            TextBlock timeBlock = new TextBlock();
            timeBlock.Text = time.ToString("t");
            timeBlock.Foreground = Brushes.White;

            int messageIndex = data.messageCollection.IndexOf(message);
            bool personChanged = messageIndex == 0 || data.messageCollection[messageIndex - 1].person.id != message.person.id;

            if (message.person.id == 0)
            {
                // 吹き出しの表示
                if (personChanged && message.Color == Original.Green)
                {
                    bubble.Source = new BitmapImage(new Uri("green bubble.png", UriKind.Relative));
                    bubble.HorizontalAlignment = HorizontalAlignment.Right;
                    bubble.Margin = new Thickness(0, messageTop, messageRight - bubble.Width / 2, 0);
                    messageGrid.Children.Add(bubble);
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                messageBorder.Margin = new Thickness(0, messageTop, messageRight, 0);

                // 時刻の表示
                if (message.Color == Original.Green)
                {
                    double timeBlockTop = messageBorder.Margin.Top + Original.GetHeight(messageBorder) - Original.GetHeight(timeBlock) - timeBottom;
                    double timeBlockRight = messageRight + Original.GetWidth(messageBorder) + timeSide;
                    timeBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    timeBlock.Margin = new Thickness(0, timeBlockTop, timeBlockRight, 0);
                    messageGrid.Children.Add(timeBlock);

                    time += TimeSpan.FromMinutes(message.timeInterval);
                }
            }
            else
            {
                if (personChanged)
                {
                    // アイコンの表示
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

                    // 名前の表示
                    TextBlock nameBlock = new TextBlock();
                    nameBlock.Text = message.person.Name;
                    nameBlock.FontSize = 20;
                    nameBlock.Foreground = Brushes.White;
                    nameBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    nameBlock.VerticalAlignment = VerticalAlignment.Top;
                    nameBlock.Margin = new Thickness(messageLeft, messageTop, 0, 0);
                    messageGrid.Children.Add(nameBlock);

                    messageTop += Original.GetHeight(nameBlock) + nameBottom;

                    // 吹き出しの表示
                    if (message.Color == Colors.White)
                    {
                        bubble.Source = new BitmapImage(new Uri("white bubble.png", UriKind.Relative));
                        bubble.HorizontalAlignment = HorizontalAlignment.Left;
                        bubble.Margin = new Thickness(messageLeft - bubble.Width / 2, messageTop, 0, 0);
                        messageGrid.Children.Add(bubble);
                    }
                }

                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
                messageBorder.Margin = new Thickness(messageLeft, messageTop, 0, 0);

                // 時刻の表示
                if (message.Color == Colors.White)
                {
                    double timeBlockTop = messageBorder.Margin.Top + Original.GetHeight(messageBorder) - Original.GetHeight(timeBlock) - timeBottom;
                    double timeBlockLeft = messageLeft + Original.GetWidth(messageBorder) + timeSide;
                    timeBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    timeBlock.Margin = new Thickness(timeBlockLeft, timeBlockTop, 0, 0);
                    messageGrid.Children.Add(timeBlock);

                    time += TimeSpan.FromMinutes(message.timeInterval);
                }
            }

            messageGrid.Children.Add(messageBorder);

            messageTop += Original.GetHeight(messageBorder) + messageDistance;

            double inputBarTop = messageGrid.ActualHeight - inputBar.ActualHeight;
            if (inputBarTop < messageTop)
            {
                double overHeight = messageTop - inputBarTop;
                messageGrid.Margin = new Thickness(messageGrid.Margin.Left, messageGrid.Margin.Top - overHeight, messageGrid.Margin.Right, messageGrid.Margin.Bottom);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"mp4|*.mp4";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    progressRing.IsActive = true;

                    // メッセージグリッドからすべての要素を削除
                    messageGrid.Children.Clear();
                    // メッセージグリッドの余白を初期化
                    messageGrid.Margin = new Thickness(0);
                    // メッセージの上の余白を初期化
                    messageTop = groupBar.ActualHeight + messageDistance;

                    // 動画
                    string videoPath = Path.Combine(tempDirectory, Guid.NewGuid() + Path.GetExtension(saveFileDialog.FileName));

                    if (backgroundType == BackgroundType.Animation)
                    {
                        // 背景動画
                        string animationPath = Path.Combine(tempDirectory, Guid.NewGuid() + Path.GetExtension(mediaElement.Source.OriginalString));
                        File.Copy(mediaElement.Source.OriginalString, animationPath, true);

                        await Task.Run(() =>
                        {
                            // 背景動画のフレームレートを変更
                            animationPath = Original.ChangeFrameRate(animationPath, frameRate);
                            // 背景動画のサイズを変更
                            animationPath = Original.ResizeVideo(animationPath, (int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight);
                            // 背景動画のループ動画を作成
                            animationPath = Original.LoopVideoOrAudio(animationPath, data.videoTotalTime);
                        });

                        DateTime time = new DateTime();
                        if (data.startTimeList.Any(s => s.Duration <= data.messageCollection.First().Duration))
                        {
                            time = data.startTimeList.OrderBy(s => s.Duration).Last(s => s.Duration <= data.messageCollection.First().Duration).Time;
                        }

                        string input = $"-i {animationPath} -i {GetMessageBitmapPath()} ";
                        string overlay = $"[0][1]overlay=enable='lt(t,{data.messageCollection.First().Duration})'[tmp];";

                        for (int i = 0; i < data.messageCollection.Count; i++)
                        {
                            if (i != 0 && data.startTimeList.Any(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration))
                            {
                                time = data.startTimeList.OrderBy(s => s.Duration).Last(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration).Time;
                            }

                            DrawMessage(data.messageCollection[i], ref time);
                            await Task.Delay(100);

                            input += $"-i {GetMessageBitmapPath()} ";
                            overlay += (i == data.messageCollection.Count - 1)
                                       ? $"[tmp][{i + 2}]overlay=enable='gte(t,{data.messageCollection[i].Duration})'"
                                       : $"[tmp][{i + 2}]overlay=enable='gte(t,{data.messageCollection[i].Duration})*lt(t,{data.messageCollection[i + 1].Duration})'[tmp];";
                        }

                        await Task.Run(() => Original.FFmpeg($"{input}" +
                                                             $"-filter_complex {overlay} " +
                                                             $"-preset ultrafast " +
                                                             $"{videoPath}"));
                    }
                    else
                    {
                        using (var writer = new VideoFileWriter())
                        {
                            writer.Open(videoPath, (int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight);

                            using (var messageBitmap = new Bitmap(GetMessageBitmapPath()))
                            {
                                writer.WriteVideoFrame(messageBitmap);
                            }

                            DateTime time = new DateTime();
                            if (data.startTimeList.Any(s => s.Duration <= data.messageCollection.First().Duration))
                            {
                                time = data.startTimeList.OrderBy(s => s.Duration).Last(s => s.Duration <= data.messageCollection.First().Duration).Time;
                            }

                            for (int i = 0; i < data.messageCollection.Count; i++)
                            {
                                if (i != 0 && data.startTimeList.Any(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration))
                                {
                                    time = data.startTimeList.OrderBy(s => s.Duration).Last(s => data.messageCollection[i - 1].Duration < s.Duration && s.Duration <= data.messageCollection[i].Duration).Time;
                                }

                                DrawMessage(data.messageCollection[i], ref time);
                                await Task.Delay(100);

                                using (var messageBitmap = new Bitmap(GetMessageBitmapPath()))
                                {
                                    writer.WriteVideoFrame(messageBitmap, TimeSpan.FromSeconds(data.messageCollection[i].Duration));
                                }
                            }

                            using (var messageBitamap = new Bitmap(GetMessageBitmapPath()))
                            {
                                writer.WriteVideoFrame(messageBitamap, TimeSpan.FromSeconds(data.videoTotalTime));
                            }
                        }

                        // 動画のフレームレートを変更
                        await Task.Run(() => videoPath = Original.ChangeFrameRate(videoPath, frameRate));
                    }

                    await Task.Run(() =>
                    {
                        // 動画のサイズを変更
                        videoPath = Original.ResizeVideo(videoPath, frameWidth, frameHeight);
                        // 動画に音声を挿入
                        videoPath = GetVideoWithAudio(videoPath);
                    });

                    File.Copy(videoPath, saveFileDialog.FileName, true);

                    progressRing.IsActive = false;
                    MessageBox.Show("保存されました");
                }
                catch
                {
                    progressRing.IsActive = false;
                    MessageBox.Show("保存できませんでした");
                }
            }

            // すべてのボタンを有効化
            foreach (var button in grid.Children.Cast<Button>())
            {
                button.IsEnabled = true;
            }
        }

        private string GetMessageBitmapPath()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(screenGrid);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            string messageBitmapPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".png");
            using (var fs = File.Create(messageBitmapPath))
            {
                encoder.Save(fs);
            }

            return messageBitmapPath;
        }

        private string GetVideoWithAudio(string videoPath)
        {
            List<AudioFileReader> tempAudioList = new List<AudioFileReader>();
            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();

            // BGM
            if (data.bgm != null)
            {
                string bgmPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(bgmPath, data.bgm);

                AudioFileReader bgmAudio = new AudioFileReader(Original.LoopVideoOrAudio(bgmPath, data.videoTotalTime));
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
                    offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Duration);
                    sampleProviderList.Add(offsetSampleProvider);
                }
            }

            // 音声
            foreach (var message in data.messageCollection.Where(m => m.Voice != null))
            {
                string voicePath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(voicePath, message.Voice);

                AudioFileReader voiceAudio = new AudioFileReader(voicePath);
                tempAudioList.Add(voiceAudio);

                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(voiceAudio);
                offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Duration);
                sampleProviderList.Add(offsetSampleProvider);
            }

            if (sampleProviderList.Count > 0)
            {
                // BGM・効果音・音声を合成
                string audioPath = Path.Combine(tempDirectory, Guid.NewGuid() + ".wav");
                MixingSampleProvider mixingSampleProvider = new MixingSampleProvider(sampleProviderList);
                WaveFileWriter.CreateWaveFile16(audioPath, mixingSampleProvider);

                foreach (var tempAudio in tempAudioList)
                {
                    tempAudio.Dispose();
                }

                // 動画に音声を挿入
                videoPath = Original.AddAudioToVideo(videoPath, audioPath);
            }

            return videoPath;
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
