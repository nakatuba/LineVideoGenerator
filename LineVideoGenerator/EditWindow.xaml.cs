using InWit.WPF.MultiRangeSlider;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private double messageTime;
        private bool isSetIcon = false;

        public EditWindow()
        {
            InitializeComponent();
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ImageBrush imageBrush = (ImageBrush)iconButton.Template.FindName("imageBrush", iconButton);
                    BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    imageBrush.ImageSource = bitmapImage;
                    isSetIcon = true;
                    CheckOtherSendButton();
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            mySendButton.IsEnabled = !string.IsNullOrWhiteSpace(myTextBox.Text);
        }

        private void OtherTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckOtherSendButton();
        }

        /*
        private void MySendButton_Click(object sender, RoutedEventArgs e)
        {
            SendText(myTextBox);
        }

        private void OtherSendButton_Click(object sender, RoutedEventArgs e)
        {
            SendText(otherTextBox);
        }
        */

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Button sendButton = sender as Button;
            TextBox messageBox = sendButton.CommandTarget as TextBox;
            int personID = Convert.ToInt32(messageBox.Tag);
            messageTime++;
            // mySlider.Maximum = messageTime;
            mySlider.Maximum = 60;
            // otherSlider.Maximum = messageTime;
            otherSlider.Maximum = 60;
            WitMultiRangeSliderItem sliderItem = new WitMultiRangeSliderItem();

            Message message;
            if (personID == 1)
            {
                message = new Message(personID, messageBox.Text, messageTime);
                mySlider.Items.Add(sliderItem);
            }
            else
            {
                ImageBrush imageBrush = (ImageBrush)iconButton.Template.FindName("imageBrush", iconButton);
                message = new Message(personID, (BitmapImage)imageBrush.ImageSource, nameTextBox.Text, messageBox.Text, messageTime);
                otherSlider.Items.Add(sliderItem);
            }

            Binding binding = new Binding("Time") { Source = message };
            sliderItem.SetBinding(RangeBase.ValueProperty, binding);

            MainWindow mainWindow = Owner as MainWindow;
            // メッセージの編集後に保存ボタンを無効化
            message.PropertyChanged += (sender2, e2) => mainWindow.saveButton.IsEnabled = false;
            mainWindow.data.messageCollection.Add(message);
            mainWindow.SendMessage(message);
            mainWindow.playButton.IsEnabled = true;

            messageBox.Text = string.Empty;
        }

        private void CheckOtherSendButton()
        {
            otherSendButton.IsEnabled = isSetIcon && !string.IsNullOrWhiteSpace(nameTextBox.Text) && !string.IsNullOrWhiteSpace(otherTextBox.Text);
        }

        /*
        private void SendText(TextBox textBox)
        {
            MainWindow mainWindow = Owner as MainWindow;

            if (textBox == myTextBox)
            {
                Message message = new Message(textBox.Text);
                message.myMessage = true;
                mainWindow.SendMessage(message);
                mainWindow.data.messageList.Add(message);
            }
            else
            {
                ImageBrush imageBrush = (ImageBrush)iconButton.Template.FindName("imageBrush", iconButton);
                Message message = new Message((BitmapImage)imageBrush.ImageSource, nameTextBox.Text, textBox.Text);
                message.myMessage = false;
                mainWindow.SendMessage(message);
                mainWindow.data.messageList.Add(message);
            }

            mainWindow.playButton.IsEnabled = true;
            mainWindow.saveButton.IsEnabled = false;
            textBox.Text = string.Empty;
        }
        */

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
            Message message = dataGrid.SelectedItem as Message;
            if (message.Voice != null)
            {
                Button playButton = sender as Button;
                playButton.IsEnabled = false;
                message.PlayVoice(() => playButton.IsEnabled = true);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageCollection.Remove((Message)dataGrid.SelectedItem);
            if (mainWindow.data.messageCollection.Count == 0)
            {
                mainWindow.playButton.IsEnabled = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
        }
    }
}
