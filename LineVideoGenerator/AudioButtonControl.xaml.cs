using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LineVideoGenerator
{
    public enum AudioType
    {
        BGM,
        SoundEffect
    }

    /// <summary>
    /// SetAudioControl.xaml の相互作用ロジック
    /// </summary>
    public partial class AudioButtonControl : UserControl
    {
        public AudioType AudioType { get; set; }

        public AudioButtonControl()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                switch (AudioType)
                {
                    case AudioType.BGM:
                        audioButton.Content = "BGM";
                        break;
                    case AudioType.SoundEffect:
                        audioButton.Content = "効果音";
                        break;
                }
            };
        }

        private void AudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    EditWindow editWindow = Window.GetWindow(this) as EditWindow;
                    MainWindow mainWindow = editWindow.Owner as MainWindow;

                    switch (AudioType)
                    {
                        case AudioType.BGM:
                            mainWindow.data.bgm = Original.GetByteArray(openFileDialog.FileName);
                            break;
                        case AudioType.SoundEffect:
                            mainWindow.data.soundEffect = Original.GetByteArray(openFileDialog.FileName);
                            break;
                    }

                    playButton.IsEnabled = true;
                    resetButton.IsEnabled = true;
                }
                catch
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = Window.GetWindow(this) as EditWindow;
            MainWindow mainWindow = editWindow.Owner as MainWindow;

            switch (AudioType)
            {
                case AudioType.BGM:
                    Original.PlayButton_Click(mainWindow.data.bgm, playButton, PlayButton_Click);
                    break;
                case AudioType.SoundEffect:
                    Original.PlayButton_Click(mainWindow.data.soundEffect, playButton, PlayButton_Click);
                    break;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = Window.GetWindow(this) as EditWindow;
            MainWindow mainWindow = editWindow.Owner as MainWindow;

            switch (AudioType)
            {
                case AudioType.BGM:
                    mainWindow.data.bgm = null;
                    break;
                case AudioType.SoundEffect:
                    mainWindow.data.soundEffect = null;
                    break;
            }

            playButton.IsEnabled = false;
            resetButton.IsEnabled = false;
        }
    }
}
