using InWit.WPF.MultiRangeSlider;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LineVideoGenerator
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // メッセージを時間順にソート（https://yomon.hatenablog.com/entry/2014/01/14/C%23%E3%81%AEObservableCollection%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E8%A6%81%E7%B4%A0%E3%81%AE%E4%B8%A6%E3%81%B9%E6%9B%BF%E3%81%88%E6%96%B9%E6%B3%95）
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageCollection = new ObservableCollection<Message>(mainWindow.data.messageCollection.OrderBy(n => n.Time));
            dataGrid.ItemsSource = mainWindow.data.messageCollection;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Message message = dataGrid.SelectedItem as Message;
                    message.Voice = File.ReadAllBytes(openFileDialog.FileName);
                    message.voicePathExt = Path.GetExtension(openFileDialog.FileName);
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button playButton = sender as Button;
            playButton.IsEnabled = false;

            Message message = dataGrid.SelectedItem as Message;
            message.PlayVoice(() => playButton.IsEnabled = true);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            Message message = dataGrid.SelectedItem as Message;
            mainWindow.data.messageCollection.Remove(message);
            if (mainWindow.data.messageCollection.Count == 0)
            {
                mainWindow.playButton.IsEnabled = false;
            }
            message.slider.Items.Remove(message.sliderItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
        }
    }
}
