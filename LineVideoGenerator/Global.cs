using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LineVideoGenerator
{
    class Global
    {
        /// <summary>
        /// TextBlock及びその親要素の幅を取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
        /// </summary>
        /// <param name="element">TextBlock及びその親要素</param>
        /// <returns>TextBlock及びその親要素の幅</returns>
        public static double GetWidth(FrameworkElement element)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            return element.ActualWidth;
        }

        /// <summary>
        /// TextBlock及びその親要素の高さを取得（https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters）
        /// </summary>
        /// <param name="element">TextBlock及びその親要素</param>
        /// <returns>TextBlock及びその親要素の高さ</returns>
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
        /// 指定した再生時間の分だけ繰り返すループ音声のパスを取得
        /// </summary>
        /// <param name="inputPath">音声のパス</param>
        /// <param name="time">音声の再生時間</param>
        /// <returns>ループ音声のパス</returns>
        public static string GetLoopAudio(string inputPath, int time)
        {
            string outputPath = Path.Combine(MainWindow.tempDirectory, Guid.NewGuid() + ".wav");
            using (var process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = $"-stream_loop -1 -i {inputPath} -t {time} {outputPath}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }

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
