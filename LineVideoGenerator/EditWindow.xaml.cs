using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace LineVideoGenerator
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : Window
    {
        public ContextMenu StartTimeMenu
        {
            get
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem menuItem = new MenuItem();
                menuItem.Header = "削除";
                menuItem.Click += (sender, e) =>
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    Thumb thumb = contextMenu.PlacementTarget as Thumb;

                    if (mainWindow.data.startTimeList.Any(s => s.thumb == thumb))
                    {
                        StartTime startTime = mainWindow.data.startTimeList.First(s => s.thumb == thumb);
                        mainWindow.data.startTimeList.Remove(startTime);
                        startTime.RemoveThumb();
                    }
                };

                contextMenu.Items.Add(menuItem);

                return contextMenu;
            }
        }

        public EditWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                SetEditWindow();
            };
        }

        private void SetEditWindow()
        {
            MainWindow mainWindow = Owner as MainWindow;
            DataContext = mainWindow.data;

            mainWindow.SetMessageCollectionCollectionChanged();

            foreach (var message in mainWindow.data.messageCollection)
            {
                SetMessagePropertyChanged(message);
            }

            foreach (var person in mainWindow.data.personList)
            {
                SetPersonPropertyChanged(person);
            }

            SetButtonControl();
            SetTimePicker();

            // アイコンと名前をセット
            foreach (var group in mainWindow.data.messageCollection.GroupBy(m => m.person.id))
            {
                SendMessageControl control = sendGrid.Children.Cast<SendMessageControl>().First(c => Grid.GetRow(c) == group.Key);
                ImageBrush imageBrush = control.iconButton.Template.FindName("imageBrush", control.iconButton) as ImageBrush;
                imageBrush.ImageSource = group.First().person.Icon;
                control.isSetIcon = true;
                control.nameBox.Text = group.First().person.Name;
            }

            // タイムスライダーをセット
            if (mainWindow.data.startTimeList.Count == 0)
            {
                mainWindow.data.startTimeList.Add(new StartTime(0, startTimeCanvas, StartTimeMenu));
            }
            else
            {
                foreach (var startTime in mainWindow.data.startTimeList)
                {
                    startTime.AddThumb(startTimeCanvas, StartTimeMenu);
                }
            }

            foreach (var message in mainWindow.data.messageCollection)
            {
                Canvas canvas = messageCanvasGrid.Children.Cast<Canvas>().First(c => Grid.GetRow(c) == message.person.id);
                message.AddThumb(canvas);
            }

            // データグリッドをセット
            dataGrid.ItemsSource = mainWindow.data.messageCollection;
        }

        public void SetMessagePropertyChanged(Message message)
        {
            MainWindow mainWindow = Owner as MainWindow;

            message.PropertyChanged += (sender, e) =>
            {
                timePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.messageCollection.Max(m => m.NextMessageDuration)));
                dataGrid.Items.Refresh();
            };
        }

        public void SetPersonPropertyChanged(Person person)
        {
            person.PropertyChanged += (sender, e) =>
            {
                dataGrid.Items.Refresh();
            };
        }

        private void SetButtonControl()
        {
            MainWindow mainWindow = Owner as MainWindow;

            bgmButtonControl.playButton.IsEnabled = mainWindow.data.bgm != null;
            bgmButtonControl.resetButton.IsEnabled = mainWindow.data.bgm != null;

            soundEffectButtonControl.playButton.IsEnabled = mainWindow.data.soundEffect != null;
            soundEffectButtonControl.resetButton.IsEnabled = mainWindow.data.soundEffect != null;
        }

        private void SetTimePicker()
        {
            MainWindow mainWindow = Owner as MainWindow;
            timePicker.Value = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.videoTotalTime));

            if (mainWindow.data.messageCollection.Count > 0)
            {
                timePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.messageCollection.Max(m => m.NextMessageDuration)));
            }
        }

        private void TimePicker_ValueChanged(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.videoTotalTime = (int)timePicker.Value.TimeOfDay.TotalSeconds;

            foreach (var canvas in messageCanvasGrid.Children.Cast<Canvas>())
            {
                canvas.Width = mainWindow.data.videoTotalTime * ThumbConverter.per;
            }

            startTimeCanvas.Width = mainWindow.data.videoTotalTime * ThumbConverter.per;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            leftTimeBlock.Text = TimeSpan.FromSeconds(scrollViewer.HorizontalOffset / ThumbConverter.per).ToString(@"mm\:ss");
            rightTimeBlock.Text = TimeSpan.FromSeconds((scrollViewer.HorizontalOffset + scrollViewer.ActualWidth) / ThumbConverter.per).ToString(@"mm\:ss");
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            Thumb thumb = sender as Thumb;

            if (mainWindow.data.messageCollection.Any(m => m.thumb == thumb))
            {
                dataGrid.SelectedItem = mainWindow.data.messageCollection.First(m => m.thumb == thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = sender as Thumb;

            double x = Canvas.GetLeft(thumb) + e.HorizontalChange;
            x = Math.Max(x, 0);
            Canvas.SetLeft(thumb, x);

            ScrollIntoThumb(thumb);

            // メッセージを時間順にソート（https://yomon.hatenablog.com/entry/2014/01/14/C%23%E3%81%AEObservableCollection%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E8%A6%81%E7%B4%A0%E3%81%AE%E4%B8%A6%E3%81%B9%E6%9B%BF%E3%81%88%E6%96%B9%E6%B3%95）
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageCollection = new ObservableCollection<Message>(mainWindow.data.messageCollection.OrderBy(m => m.Duration));
            mainWindow.SetMessageCollectionCollectionChanged();
            dataGrid.ItemsSource = mainWindow.data.messageCollection;

            // ドラッグ中のメッセージが表示されるようデータグリッドをスクロール
            if (mainWindow.data.messageCollection.Any(m => m.thumb == thumb))
            {
                dataGrid.ScrollIntoView(mainWindow.data.messageCollection.First(m => m.thumb == thumb));
            }
        }

        private void StartTimeCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MainWindow mainWindow = Owner as MainWindow;
                int duration = (int)e.GetPosition(startTimeCanvas).X / ThumbConverter.per;
                mainWindow.data.startTimeList.Add(new StartTime(duration, startTimeCanvas, StartTimeMenu));
            }

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is Message selectedMessage)
            {
                MainWindow mainWindow = Owner as MainWindow;
                foreach (var message in mainWindow.data.messageCollection)
                {
                    message.thumb.BorderThickness = new Thickness();
                }

                selectedMessage.thumb.BorderThickness = new Thickness(2);

                ScrollIntoThumb(selectedMessage.thumb);
                dataGrid.ScrollIntoView(selectedMessage);
            }
        }

        private void ScrollIntoThumb(Thumb thumb)
        {
            if (Canvas.GetLeft(thumb) < scrollViewer.HorizontalOffset)
            {
                scrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb));
            }
            else if (scrollViewer.HorizontalOffset + scrollViewer.ActualWidth < Canvas.GetLeft(thumb) + thumb.ActualWidth)
            {
                scrollViewer.ScrollToHorizontalOffset(Canvas.GetLeft(thumb) + thumb.ActualWidth - scrollViewer.ActualWidth);
            }
        }

        private void ColorPicker_Opened(object sender, RoutedEventArgs e)
        {
            ColorPicker colorPicker = sender as ColorPicker;
            ColorItem white = new ColorItem(Colors.White, "White");
            ColorItem green = new ColorItem(Original.Green, "Green");
            ObservableCollection<ColorItem> defaultColors = new ObservableCollection<ColorItem>() { white, green };
            colorPicker.AvailableColors = defaultColors;
        }

        private void EditMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Button editButton = sender as Button;
            editButton.IsEnabled = false;

            Message message = dataGrid.SelectedItem as Message;
            EditMessageWindow editMessageWindow = new EditMessageWindow(message);
            editMessageWindow.Owner = this;
            editMessageWindow.Closed += (sender2, e2) => { editButton.IsEnabled = true; };
            editMessageWindow.ShowDialog();
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Message message = dataGrid.SelectedItem as Message;
                    message.Voice = Original.GetByteArray(openFileDialog.FileName);
                    AudioFileReader audioFileReader = new AudioFileReader(openFileDialog.FileName);
                    message.VoiceTime = audioFileReader.TotalTime.TotalSeconds;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void PlayVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Message message = dataGrid.SelectedItem as Message;
            Button playVoiceButton = sender as Button;
            Original.PlayButton_Click(message.Voice, playVoiceButton, PlayVoiceButton_Click);
        }

        private void ResetVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Message message = dataGrid.SelectedItem as Message;
            message.Voice = null;
            message.VoiceTime = 1;
        }

        private void DeleteMessageButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;

            if (dataGrid.SelectedItem is Message message)
            {
                mainWindow.data.messageCollection.Remove(message);
                message.RemoveThumb();
            }

            if (mainWindow.data.messageCollection.Count > 0)
            {
                timePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(mainWindow.data.messageCollection.Max(m => m.NextMessageDuration)));
            }
            else
            {
                timePicker.MinDate = DateTime.Today;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml|*.xml";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ResetSendGrid();
                    ResetTimeSlider();
                    ResetTimePicker();

                    MainWindow mainWindow = Owner as MainWindow;

                    // データを読み込み
                    XmlSerializer se = new XmlSerializer(typeof(Data));
                    using (var fs = File.OpenRead(openFileDialog.FileName))
                    {
                        mainWindow.data = se.Deserialize(fs) as Data;
                    }

                    // メッセージがあれば再生ボタンと保存ボタンを有効化し、メッセージがなければ無効化
                    mainWindow.playButton.IsEnabled = mainWindow.data.messageCollection.Count > 0;
                    mainWindow.saveButton.IsEnabled = mainWindow.data.messageCollection.Count > 0;

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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "xml|*.xml";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindow mainWindow = Owner as MainWindow;
                    progressRing.IsActive = true;

                    await Task.Run(() =>
                    {
                        // データを保存
                        XmlSerializer se = new XmlSerializer(typeof(Data));
                        using (var fs = File.Create(saveFileDialog.FileName))
                        {
                            se.Serialize(fs, mainWindow.data);
                        }
                    });

                    progressRing.IsActive = false;
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
            MessageBoxResult result = MessageBox.Show("現在編集中のデータは失われます", "リセット", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                ResetSendGrid();
                ResetTimePicker();
                ResetTimeSlider();

                MainWindow mainWindow = Owner as MainWindow;
                mainWindow.data = new Data();
                mainWindow.playButton.IsEnabled = false;
                mainWindow.saveButton.IsEnabled = false;
                mainWindow.SetMessageCollectionCollectionChanged();
                SetButtonControl();
                SetTimePicker();
                dataGrid.ItemsSource = mainWindow.data.messageCollection;
            }
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

        private void ResetTimePicker()
        {
            timePicker.MinDate = DateTime.Today;
        }

        private void ResetTimeSlider()
        {
            MainWindow mainWindow = Owner as MainWindow;

            foreach (var startTime in mainWindow.data.startTimeList)
            {
                startTime.RemoveThumb();
            }

            foreach (var message in mainWindow.data.messageCollection)
            {
                message.RemoveThumb();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
            ResetTimeSlider();
        }
    }
}
