using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LineVideoGenerator
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : Window
    {
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
            Message message;

            if (personID == 1)
            {
                message = new Message(personID, messageBox.Text);

            }
            else
            {
                ImageBrush imageBrush = (ImageBrush)iconButton.Template.FindName("imageBrush", iconButton);
                message = new Message(personID, (BitmapImage)imageBrush.ImageSource, nameTextBox.Text, messageBox.Text);
            }

            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.data.messageList.Add(message);
            mainWindow.SendMessage(message);
            mainWindow.playButton.IsEnabled = true;
            mainWindow.saveButton.IsEnabled = false;

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

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
        }
    }
}
