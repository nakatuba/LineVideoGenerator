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
        private List<Bitmap> messageBitmapList = new List<Bitmap>();
        public static string tempDirectory = "temp";
        public string backgroundPath = "background.png";
        public BackgroundType backgroundType = BackgroundType.Default;
        public Data data = new Data();
        public int FrameRate
        {
            get
            {
                if (backgroundType == BackgroundType.Animation)
                {
                    using (var reader = new VideoFileReader())
                    {
                        reader.Open(backgroundPath);
                        return (int)reader.FrameRate;
                    }
                }
                else
                {
                    return 25;
                }
            }
        }

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

            // 編集画面と設定画面を閉じる
            Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.GetType() == typeof(EditWindow))?.Close();
            Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.GetType() == typeof(BackgroundWindow))?.Close();

            // 再生ボタンを停止ボタンに変更
            playButton.Content = "停止";
            playButton.Click -= PlayButton_Click;
            playButton.Click += StopButton_Click;

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

            // 停止ボタンを再生ボタンに戻す
            playButton.Content = "再生";
            playButton.Click -= StopButton_Click;
            playButton.Click += PlayButton_Click;

            // 再生ボタン以外のボタンを有効化
            foreach (var button in grid.Children.Cast<Button>().Where(b => b != playButton))
            {
                button.IsEnabled = true;
            }
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
            saveFileDialog.DefaultExt = ".avi";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // メッセージグリッドからすべての要素を削除
                    messageGrid.Children.Clear();
                    // メッセージグリッドの余白を初期化
                    messageGrid.Margin = new Thickness(0);
                    // メッセージの上の余白を初期化
                    messageTop = groupBar.ActualHeight + messageDistance;

                    for (int i = 0; i < data.messageCollection.Count; i++)
                    {
                        if (i == 0)
                        {
                            TimeSpan firstMessageTimeSpan = TimeSpan.FromSeconds(data.messageCollection[i].Time);
                            AddMessageBitmap(firstMessageTimeSpan);
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
                        await Task.Delay(10);
                        AddMessageBitmap(messageTimeSpan);
                    }

                    progressRing.IsActive = true;

                    // 動画を保存
                    await Task.Run(() =>
                    {
                        if (backgroundType == BackgroundType.Animation) SaveAnimationBackground(saveFileDialog.FileName);
                        else SaveImageBackground(saveFileDialog.FileName);
                        AddVideoAudio(saveFileDialog.FileName);
                    });

                    progressRing.IsActive = false;
                    MessageBox.Show("保存されました");

                    foreach (var messageBitmap in messageBitmapList)
                    {
                        messageBitmap.Dispose();
                    }
                    messageBitmapList.Clear();
                }
                catch (Exception)
                {
                    MessageBox.Show("保存できませんでした");
                }
            }
        }

        private void AddMessageBitmap(TimeSpan messageTimeSpan)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)screenGrid.ActualWidth, (int)screenGrid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(screenGrid);
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            string messagePath = Path.Combine(tempDirectory, Guid.NewGuid() + ".png");
            using (var fileStream = File.Create(messagePath))
            {
                pngBitmapEncoder.Save(fileStream);
            }

            Bitmap messageBitmap = new Bitmap(messagePath);
            for (int j = 0; j < messageTimeSpan.TotalSeconds * FrameRate; j++)
            {
                messageBitmapList.Add(messageBitmap);
            }
        }

        private void SaveImageBackground(string fileName)
        {
            using (var writer = new VideoFileWriter())
            {
                writer.Open(fileName, frameWidth, frameHeight, FrameRate);

                for (int i = 0; i < messageBitmapList.Count; i++)
                {
                    using (var backgroundBitmap = new Bitmap(backgroundPath))
                    using (var frameBitmap = new Bitmap(frameWidth, frameHeight))
                    using (var graphics = Graphics.FromImage(frameBitmap))
                    {
                        graphics.DrawImage(backgroundBitmap, 0, 0, frameBitmap.Width, frameBitmap.Height);
                        graphics.DrawImage(messageBitmapList[i], 0, 0, frameBitmap.Width, frameBitmap.Height);

                        writer.WriteVideoFrame(frameBitmap);
                    }
                }
            }
        }

        private void SaveAnimationBackground(string fileName)
        {
            using (var reader = new VideoFileReader())
            using (var writer = new VideoFileWriter())
            {
                reader.Open(backgroundPath);
                writer.Open(fileName, frameWidth, frameHeight, FrameRate);

                for (int i = 0; i < messageBitmapList.Count; i++)
                {
                    using (var backgroundBitmap = reader.ReadVideoFrame())
                    using (var frameBitmap = new Bitmap(frameWidth, frameHeight))
                    using (var graphics = Graphics.FromImage(frameBitmap))
                    {
                        graphics.DrawImage(backgroundBitmap, 0, 0, frameBitmap.Width, frameBitmap.Height);
                        graphics.DrawImage(messageBitmapList[i], 0, 0, frameBitmap.Width, frameBitmap.Height);

                        writer.WriteVideoFrame(frameBitmap);

                        if (i % reader.FrameCount == 0)
                        {
                            reader.Close();
                            reader.Open(backgroundPath);
                        }
                    }
                }
            }
        }

        private void AddVideoAudio(string fileName)
        {
            List<ISampleProvider> sampleProviderList = new List<ISampleProvider>();

            // BGM
            if (data.bgm != null)
            {
                string bgmPath = Path.Combine(tempDirectory, Guid.NewGuid() + "wav");
                File.WriteAllBytes(bgmPath, data.bgm);
                AudioFileReader bgmAudio = new AudioFileReader(bgmPath);

                List<ISampleProvider> bgmList = new List<ISampleProvider>();
                for (int i = 0; i < (int)(data.videoTotalTime / bgmAudio.TotalTime.TotalSeconds); i++)
                {
                    bgmList.Add(new AudioFileReader(bgmPath));
                }

                OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(bgmAudio);
                offsetSampleProvider.Take = TimeSpan.FromSeconds(data.videoTotalTime % bgmAudio.TotalTime.TotalSeconds);
                bgmList.Add(offsetSampleProvider);

                ConcatenatingSampleProvider concatenatingSampleProvider = new ConcatenatingSampleProvider(bgmList);
                sampleProviderList.Add(concatenatingSampleProvider);
            }

            // 効果音
            if (data.soundEffect != null)
            {
                foreach (var message in data.messageCollection)
                {
                    string soundEffectPath = Path.Combine(tempDirectory, Guid.NewGuid() + "wav");
                    File.WriteAllBytes(soundEffectPath, data.soundEffect);
                    AudioFileReader soundEffectAudio = new AudioFileReader(soundEffectPath);

                    OffsetSampleProvider offsetSampleProvider = new OffsetSampleProvider(soundEffectAudio);
                    offsetSampleProvider.DelayBy = TimeSpan.FromSeconds(message.Time);
                    sampleProviderList.Add(offsetSampleProvider);
                }
            }

            // 音声
            foreach (var message in data.messageCollection.Where(m => m.IsSetVoice))
            {
                string voicePath = Path.Combine(tempDirectory, Guid.NewGuid() + "wav");
                File.WriteAllBytes(voicePath, message.voice);
                AudioFileReader voiceAudio = new AudioFileReader(voicePath);

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
            Directory.Delete(tempDirectory, true);
        }
    }
}
