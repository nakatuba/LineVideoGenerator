using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LineVideoGenerator
{
    /// <summary>
    /// SendMessageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SendMessageControl : UserControl
    {
        private bool IsSetIcon
        {
            get
            {
                ImageBrush imageBrush = iconButton.Template.FindName("imageBrush", iconButton) as ImageBrush;
                return imageBrush.ImageSource != new BitmapImage(new Uri("no image.png", UriKind.Relative));
            }
        }
        private bool CanSendMessage
        {
            get
            {
                if (Grid.GetRow(this) == 0)
                {
                    return !string.IsNullOrWhiteSpace(messageBox.Text);
                }
                else
                {
                    return IsSetIcon && !string.IsNullOrWhiteSpace(nameBox.Text) && !string.IsNullOrWhiteSpace(messageBox.Text);
                }
            }
        }

        public SendMessageControl()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                if (Grid.GetRow(this) == 0)
                {
                    grid.Children.Remove(iconButton);
                    grid.Children.Remove(nameBox);
                }
            };
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ImageBrush imageBrush = iconButton.Template.FindName("imageBrush", iconButton) as ImageBrush;
                    BitmapImage icon = new BitmapImage(new Uri(openFileDialog.FileName));
                    imageBrush.ImageSource = icon;

                    // 同じidの人物のアイコンを変更
                    EditWindow editWindow = Window.GetWindow(this) as EditWindow;
                    MainWindow mainWindow = editWindow.Owner as MainWindow;
                    foreach (var person in mainWindow.data.personList.Where(p => p.id == Grid.GetRow(this)))
                    {
                        person.Icon = icon;
                    }

                    sendButton.IsEnabled = CanSendMessage;
                }
                catch
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (nameBox.IsFocused)
            {
                // 同じidの人物の名前を変更
                EditWindow editWindow = Window.GetWindow(this) as EditWindow;
                MainWindow mainWindow = editWindow.Owner as MainWindow;
                foreach (var person in mainWindow.data.personList.Where(p => p.id == Grid.GetRow(this)))
                {
                    person.Name = nameBox.Text;
                }

                sendButton.IsEnabled = CanSendMessage;
            }
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            sendButton.IsEnabled = CanSendMessage;
        }

        private void MessageBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (sendButton.IsEnabled) sendButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = Window.GetWindow(this) as EditWindow;
            MainWindow mainWindow = editWindow.Owner as MainWindow;

            // 同じidの人物がいなければ人物を追加
            ImageBrush imageBrush = iconButton.Template.FindName("imageBrush", iconButton) as ImageBrush;
            BitmapImage icon = imageBrush.ImageSource as BitmapImage;
            if (!mainWindow.data.personList.Any(p => p.id == Grid.GetRow(this)))
            {
                mainWindow.data.personList.Add(new Person(Grid.GetRow(this), nameBox.Text, icon));
            }

            Person person = mainWindow.data.personList.First(p => p.id == Grid.GetRow(this));
            editWindow.SetPersonPropertyChanged(person);

            int duration = 1;
            if (mainWindow.data.messageCollection.Count > 0)
            {
                duration = mainWindow.data.messageCollection.Last().NextMessageDuration;
            }

            Canvas canvas = editWindow.messageCanvasGrid.Children.Cast<Canvas>().First(c => Grid.GetRow(c) == Grid.GetRow(this));

            Message message = new Message(person, messageBox.Text, duration, canvas);
            editWindow.SetMessagePropertyChanged(message);
            editWindow.totalTimePicker.MinDate = DateTime.Today.Add(TimeSpan.FromSeconds(message.NextMessageDuration));

            message.thumb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            message.thumb.Arrange(new Rect(message.thumb.DesiredSize));
            editWindow.dataGrid.SelectedItem = message;

            mainWindow.data.messageCollection.Add(message);

            messageBox.Text = string.Empty;
        }
    }
}
