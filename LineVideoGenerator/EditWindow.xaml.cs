using InWit.WPF.MultiRangeSlider;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
            ContentRendered += (sender, e) =>
            {
                SetEditWindow();
            };
        }

        private async void SetEditWindow()
        {
            MainWindow mainWindow = Owner as MainWindow;

            mainWindow.SetMessageCollectionChanged();

            foreach (var message in mainWindow.data.messageCollection)
            {
                SetMessagePropertyChanged(message);
            }

            foreach (var person in mainWindow.data.personList)
            {
                SetPersonPropertyChanged(person);
            }

            // アイコンと名前をセット
            foreach (var group in mainWindow.data.messageCollection.GroupBy(m => m.person.id))
            {
                SendMessageControl control = sendGrid.Children.Cast<SendMessageControl>().First(c => Grid.GetRow(c) == group.Key);
                ImageBrush imageBrush = control.iconButton.Template.FindName("imageBrush", control.iconButton) as ImageBrush;
                imageBrush.ImageSource = group.First().person.Icon;
                control.isSetIcon = true;
                control.nameBox.Text = group.First().person.Name;
            }

            // タイムピッカーをセット
            dateTimePicker.Value = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.videoTotalTime));
            if (mainWindow.data.messageCollection.Count > 0)
            {
                dateTimePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.messageCollection.Last().Time));
            }

            // タイムスライダーをセット
            foreach (var message in mainWindow.data.messageCollection)
            {
                WitMultiRangeSlider slider = sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == message.person.id);
                message.AddSliderItem(slider);
                await Task.Delay(1);
            }

            // データグリッドをセット
            dataGrid.ItemsSource = mainWindow.data.messageCollection;
        }

        /// <summary>
        /// メッセージの編集後に保存ボタンを無効化し、データグリッドを更新するよう設定
        /// </summary>
        public void SetMessagePropertyChanged(Message message)
        {
            MainWindow mainWindow = Owner as MainWindow;

            message.PropertyChanged += (sender, e) =>
            {
                mainWindow.saveButton.IsEnabled = false;
                dataGrid.Items.Refresh();
                if (e.PropertyName == "Time")
                {
                    dateTimePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.messageCollection.Last().Time));
                    dataGrid.SelectedItem = message;
                }
            };
        }

        /// <summary>
        /// 人物の編集後に保存ボタンを無効化し、データグリッドを更新するよう設定
        /// </summary>
        public void SetPersonPropertyChanged(Person person)
        {
            MainWindow mainWindow = Owner as MainWindow;

            person.PropertyChanged += (sender, e) =>
            {
                mainWindow.saveButton.IsEnabled = false;
                dataGrid.Items.Refresh();
            };
        }

        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.videoTotalTime = (int)dateTimePicker.Value.TimeOfDay.TotalSeconds;

            foreach (var slider in sliderGrid.Children.Cast<WitMultiRangeSlider>())
            {
                slider.Maximum = dateTimePicker.Value.TimeOfDay.TotalSeconds;
            }
        }

        private void Slider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // メッセージを時間順にソート（https://yomon.hatenablog.com/entry/2014/01/14/C%23%E3%81%AEObservableCollection%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E8%A6%81%E7%B4%A0%E3%81%AE%E4%B8%A6%E3%81%B9%E6%9B%BF%E3%81%88%E6%96%B9%E6%B3%95）
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageCollection = new ObservableCollection<Message>(mainWindow.data.messageCollection.OrderBy(m => m.Time));
            dataGrid.ItemsSource = mainWindow.data.messageCollection;

            // 選択項目が表示されるようデータグリッドをスクロール
            if (dataGrid.SelectedItem != null)
            {
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button editButton = sender as Button;
            editButton.IsEnabled = false;

            Message message = dataGrid.SelectedItem as Message;
            EditMessageWindow editMessageWindow = new EditMessageWindow(message);
            editMessageWindow.Owner = this;
            editMessageWindow.Closed += (sender2, e2) => { editButton.IsEnabled = true; };
            editMessageWindow.Show();
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
            WitMultiRangeSlider slider = sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == message.person.id);
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
                    ResetSendGrid();
                    ResetSliderGrid();
                    ResetTimePicker();

                    // データを読み込み
                    MainWindow mainWindow = Owner as MainWindow;
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.OpenRead(openFileDialog.FileName))
                    {
                        mainWindow.data = se.Deserialize(fs) as Data;
                    }

                    // メッセージがあれば再生ボタンを有効化
                    mainWindow.playButton.IsEnabled = mainWindow.data.messageCollection.Count > 0;

                    // メッセージと人物の対応付け
                    foreach (var message in mainWindow.data.messageCollection)
                    {
                        message.person = mainWindow.data.personList.First(p => p.id == message.person.id);
                    }

                    SetEditWindow();
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSendGrid();
            ResetSliderGrid();
            ResetTimePicker();

            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data = new Data();
            mainWindow.SetMessageCollectionChanged();
            dataGrid.ItemsSource = mainWindow.data.messageCollection;
        }

        private void ResetSendGrid()
        {
            foreach (var control in sendGrid.Children.Cast<SendMessageControl>())
            {
                ImageBrush imageBrush = control.iconButton.Template.FindName("imageBrush", control.iconButton) as ImageBrush;
                imageBrush.ImageSource = new BitmapImage(new Uri("no image.png", UriKind.Relative));
                control.nameBox.Text = string.Empty;
                control.messageBox.Text = string.Empty;
            }
        }

        private void ResetSliderGrid()
        {
            MainWindow mainWindow = Owner as MainWindow;
            foreach (var message in mainWindow.data.messageCollection)
            {
                WitMultiRangeSlider slider = sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == message.person.id);
                message.RemoveSliderItem(slider);
            }
        }

        private void ResetTimePicker()
        {
            dateTimePicker.MinDate = DateTime.Today;
            dateTimePicker.Value = DateTime.Today;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
            ResetSliderGrid();
        }
    }
}
