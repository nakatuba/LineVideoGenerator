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
using System.Windows.Shapes;
using System.Windows.Threading;

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

        private void MySendButton_Click(object sender, RoutedEventArgs e)
        {
            SendText(myTextBox);
        }

        private void OtherSendButton_Click(object sender, RoutedEventArgs e)
        {
            SendText(otherTextBox);
        }

        private void CheckOtherSendButton()
        {
            otherSendButton.IsEnabled = isSetIcon && !string.IsNullOrWhiteSpace(nameTextBox.Text) && !string.IsNullOrWhiteSpace(otherTextBox.Text);
        }

        private void SendText(TextBox textBox)
        {
            MainWindow mainWindow = Owner as MainWindow;

            if (textBox == myTextBox)
            {
                mainWindow.SendMessage(null, null, textBox.Text);
                mainWindow.messageList.Add((null, null, textBox.Text));
            }
            else
            {
                ImageBrush imageBrush = (ImageBrush)iconButton.Template.FindName("imageBrush", iconButton);
                mainWindow.SendMessage((BitmapImage)imageBrush.ImageSource, nameTextBox.Text, textBox.Text);
                mainWindow.messageList.Add(((BitmapImage)imageBrush.ImageSource, nameTextBox.Text, textBox.Text));
            }

            mainWindow.playButton.IsEnabled = true;
            mainWindow.saveButton.IsEnabled = false;
            textBox.Text = string.Empty;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.editButton.IsEnabled = true;
        }
    }
}
