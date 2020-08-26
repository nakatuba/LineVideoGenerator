using InWit.WPF.MultiRangeSlider;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LineVideoGenerator
{
    /// <summary>
    /// SendMessageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SendMessageControl : UserControl
    {
        private bool isSetIcon = false;
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
                    return isSetIcon && !string.IsNullOrWhiteSpace(nameBox.Text) && !string.IsNullOrWhiteSpace(messageBox.Text);
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
                    BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    imageBrush.ImageSource = bitmapImage;
                    isSetIcon = true;
                    sendButton.IsEnabled = CanSendMessage;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            sendButton.IsEnabled = CanSendMessage;
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            sendButton.IsEnabled = CanSendMessage;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush imageBrush = iconButton.Template.FindName("imageBrush", iconButton) as ImageBrush;
            BitmapImage icon = imageBrush.ImageSource as BitmapImage;

            EditWindow editWindow = Window.GetWindow(this) as EditWindow;
            MainWindow mainWindow = editWindow.Owner as MainWindow;

            int time = 1;
            if (mainWindow.data.messageCollection.Count > 0)
            {
                time += mainWindow.data.messageCollection.Last().Time;
            }

            WitMultiRangeSlider slider = editWindow.sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == Grid.GetRow(this));

            Message message = (Grid.GetRow(this) == 0) ? new Message(messageBox.Text, time, slider) : new Message(Grid.GetRow(this), icon, nameBox.Text, messageBox.Text, time, slider);

            // メッセージの編集後に保存ボタンを無効化し、データグリッドを更新するよう設定
            message.PropertyChanged += (sender2, e2) =>
            {
                mainWindow.saveButton.IsEnabled = false;
                editWindow.dataGrid.Items.Refresh();
            };

            mainWindow.data.messageCollection.Add(message);
            // mainWindow.SendMessage(message);

            messageBox.Text = string.Empty;
        }
    }
}
