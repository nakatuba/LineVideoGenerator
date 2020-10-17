using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LineVideoGenerator
{
    class Original
    {
        public static Color Green
        {
            get { return Color.FromRgb(112, 222, 82); }
        }

        /// <summary>
        /// TextBlockの幅を取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
        /// </summary>
        /// <param name="element">TextBlock</param>
        /// <returns>TextBlockの幅</returns>
        public static double GetWidth(FrameworkElement element)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            return element.ActualWidth;
        }

        /// <summary>
        /// TextBlockの高さを取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
        /// </summary>
        /// <param name="element">TextBlock</param>
        /// <returns>TextBlockの高さ</returns>
        public static double GetHeight(FrameworkElement element)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            return element.ActualHeight;
        }

        /// <summary>
        /// byte[]を再生
        /// </summary>
        /// <param name="bytes">再生する音声のbyte[]</param>
        public static void PlayByteArray(byte[] bytes)
        {
            if (bytes != null)
            {
                string path = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(path, bytes);

                AudioFileReader audioFileReader = new AudioFileReader(path);
                WaveOut waveOut = new WaveOut();
                waveOut.Init(audioFileReader);
                waveOut.Play();
                waveOut.PlaybackStopped += (sender, e) => audioFileReader.Dispose();
            }
        }

        /// <summary>
        /// FFmpegを起動
        /// </summary>
        /// <param name="arguments">引数</param>
        public static void FFmpeg(string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// 動画のフレームレートを変更し、そのパスを取得
        /// </summary>
        /// <param name="videoPath">動画のパス</param>
        /// <param name="frameRate">フレームレート</param>
        /// <returns>フレームレートを変更した動画のパス</returns>
        public static string ChangeFrameRate(string videoPath, int frameRate)
        {
            string outputPath = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + Path.GetExtension(videoPath));
            FFmpeg($"-i {videoPath} " +
                   $"-r {frameRate} " +
                   $"{outputPath}");

            return outputPath;
        }

        /// <summary>
        /// 動画のサイズを変更し、そのパスを取得
        /// </summary>
        /// <param name="videoPath">動画のパス</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>サイズを変更した動画のパス</returns>
        public static string ResizeVideo(string videoPath, int width, int height)
        {
            string outputPath = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + Path.GetExtension(videoPath));
            FFmpeg($"-i {videoPath} " +
                   $"-s {width}x{height} " +
                   $"{outputPath}");

            return outputPath;
        }

        /// <summary>
        /// 指定した再生時間の分だけ繰り返すループ動画（音声）を作成し、そのパスを取得（https://nico-lab.net/input_infinity_with_ffmpeg/）
        /// </summary>
        /// <param name="inputPath">動画（音声）のパス</param>
        /// <param name="time">再生時間</param>
        /// <returns>ループ動画（音声）のパス</returns>
        public static string LoopVideoOrAudio(string inputPath, int time)
        {
            string outputPath = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + Path.GetExtension(inputPath));
            FFmpeg($"-stream_loop -1 " +
                   $"-i {inputPath} " +
                   $"-c copy " +
                   $"-t {time} " +
                   $"{outputPath}");

            return outputPath;
        }

        /// <summary>
        /// 動画に音声を加え、そのパスを取得（https://qiita.com/niusounds/items/f69a4438f52fbf81f0bd）
        /// </summary>
        /// <param name="videoPath">動画のパス</param>
        /// <param name="audioPath">音声のパス</param>
        /// <param name="bitRate">ビットレート</param>
        /// <param name="frameWidth">フレームの幅</param>
        /// <param name="frameHeight">フレームの高さ</param>
        /// <returns>音声を加えた動画のパス</returns>
        public static string AddAudioToVideo(string videoPath, string audioPath)
        {
            string outputPath = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + Path.GetExtension(videoPath));
            FFmpeg($"-i {videoPath} -i {audioPath} " +
                   $"-c:v copy -c:a aac " +
                   $"-map 0:v:0 -map 1:a:0 " +
                   $"{outputPath}");

            return outputPath;
        }

        /// <summary>
        /// 音声をWAV形式に変換し、byte[]を取得
        /// </summary>
        /// <param name="fileName">音声のパス</param>
        /// <returns>WAV形式に変換した音声のbyte[]</returns>
        public static byte[] GetByteArray(string fileName)
        {
            string path = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + ".wav");

            // 音声をWAV形式に変換（https://so-zou.jp/software/tech/programming/c-sharp/media/audio/naudio/）
            using (var reader = new MediaFoundationReader(fileName))
            using (var resampler = new MediaFoundationResampler(reader, new WaveFormat()))
            {
                WaveFileWriter.CreateWaveFile(path, new MediaFoundationResampler(reader, new WaveFormat()));
            }

            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// 再生ボタンを停止ボタンに変更し、再生ボタン以外のボタンを無効化
        /// </summary>
        /// <param name="bytes">再生する音声のbyte[]</param>
        /// <param name="playButton">再生ボタン</param>
        /// <param name="PlayButton_Click">再生ボタンのメソッド</param>
        public static void PlayButton_Click(byte[] bytes, Button playButton, RoutedEventHandler PlayButton_Click)
        {
            if (bytes != null)
            {
                string path = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + ".wav");
                File.WriteAllBytes(path, bytes);

                AudioFileReader audioFileReader = new AudioFileReader(path);
                WaveOut waveOut = new WaveOut();
                waveOut.Init(audioFileReader);
                waveOut.Play();

                void StopButton_Click(object sender, RoutedEventArgs e) => waveOut.Stop();

                playButton.Content = "停止";
                playButton.Click -= PlayButton_Click;
                playButton.Click += StopButton_Click;

                Grid grid = playButton.Parent as Grid;
                foreach (var button in grid.Children.Cast<Button>().Where(b => b != playButton))
                {
                    button.IsEnabled = false;
                }

                waveOut.PlaybackStopped += (sender, e) =>
                {
                    audioFileReader.Dispose();

                    playButton.Content = "再生";
                    playButton.Click -= StopButton_Click;
                    playButton.Click += PlayButton_Click;

                    foreach (var button in grid.Children.Cast<Button>().Where(b => b != playButton))
                    {
                        button.IsEnabled = true;
                    }
                };
            }
        }
    }
}
