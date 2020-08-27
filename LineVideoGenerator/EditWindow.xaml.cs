using InWit.WPF.MultiRangeSlider;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
using System.Xml.Serialization;

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
            Loaded += (sender, e) =>
            {
                MainWindow mainWindow = Owner as MainWindow;
                dataGrid.ItemsSource = mainWindow.data.messageCollection;
            };
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // メッセージを時間順にソート（https://yomon.hatenablog.com/entry/2014/01/14/C%23%E3%81%AEObservableCollection%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E8%A6%81%E7%B4%A0%E3%81%AE%E4%B8%A6%E3%81%B9%E6%9B%BF%E3%81%88%E6%96%B9%E6%B3%95）
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageCollection = new ObservableCollection<Message>(mainWindow.data.messageCollection.OrderBy(m => m.Time));
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
            WitMultiRangeSlider slider = sliderGrid.Children.Cast<UIElement>().First(e2 => Grid.GetRow(e2) == message.person.id) as WitMultiRangeSlider;
            message.RemoveSliderItem(slider);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files|*.xml";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // データを読み込み
                    MainWindow mainWindow = Owner as MainWindow;
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.OpenRead(openFileDialog.FileName))
                    {
                        mainWindow.data = se.Deserialize(fs) as Data;
                    }

                    // メッセージがあれば再生ボタンを有効化
                    mainWindow.playButton.IsEnabled = mainWindow.data.messageCollection.Count > 0;

                    // メッセージの追加・削除後に保存ボタンを無効化し、メッセージがあれば再生ボタンを有効化するよう設定
                    mainWindow.data.messageCollection.CollectionChanged += (sender2, e2) =>
                    {
                        mainWindow.saveButton.IsEnabled = false;
                        mainWindow.playButton.IsEnabled = mainWindow.data.messageCollection.Count > 0;
                    };

                    // メッセージの編集後に保存ボタンを無効化し、データグリッドを更新するよう設定
                    foreach (var message in mainWindow.data.messageCollection)
                    {
                        message.PropertyChanged += (sender2, e2) =>
                        {
                            mainWindow.saveButton.IsEnabled = false;
                            dataGrid.Items.Refresh();
                        };
                    }

                    // 人物の編集後に保存ボタンを無効化し、データグリッドを更新するよう設定
                    foreach (var person in mainWindow.data.personList)
                    {
                        person.PropertyChanged += (sender2, e2) =>
                        {
                            mainWindow.saveButton.IsEnabled = false;
                            dataGrid.Items.Refresh();
                        };
                    }

                    // アイコンと名前をセット
                    foreach (var group in mainWindow.data.messageCollection.GroupBy(m => m.person.id))
                    {
                        SendMessageControl control = sendGrid.Children.Cast<SendMessageControl>().First(c => Grid.GetRow(c) == group.Key);
                        ImageBrush imageBrush = control.iconButton.Template.FindName("imageBrush", control.iconButton) as ImageBrush;
                        imageBrush.ImageSource = group.First().person.Icon;
                        control.nameBox.Text = group.First().person.Name;
                    }

                    // タイムスライダーをセット
                    foreach (var message in mainWindow.data.messageCollection)
                    {
                        WitMultiRangeSlider slider = sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == message.person.id);
                        message.AddSliderItem(slider);
                    }
                    
                    // データグリッドをセット
                    dataGrid.ItemsSource = mainWindow.data.messageCollection;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".xml";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // データを保存
                    MainWindow mainWindow = Owner as MainWindow;
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.Create(saveFileDialog.FileName))
                    {
                        se.Serialize(fs, mainWindow.data);
                    }

                    MessageBox.Show("保存されました");
                }
                catch (Exception)
                {
                    MessageBox.Show("保存できませんでした");
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
        }
    }
}
