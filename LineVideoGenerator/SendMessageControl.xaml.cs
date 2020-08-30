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
        public bool isSetIcon = false;
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
                    BitmapImage icon = new BitmapImage(new Uri(openFileDialog.FileName));
                    imageBrush.ImageSource = icon;
                    isSetIcon = true;

                    // 同じidの人物のアイコンを変更
                    EditWindow editWindow = Window.GetWindow(this) as EditWindow;
                    MainWindow mainWindow = editWindow.Owner as MainWindow;
                    foreach (var person in mainWindow.data.personList.Where(p => p.id == Grid.GetRow(this)))
                    {
                        person.Icon = icon;
                    }

                    sendButton.IsEnabled = CanSendMessage;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("異なる形式を選択してください");
                }
            }
        }

        public void NameBox_TextChanged(object sender, TextChangedEventArgs e)
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

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            sendButton.IsEnabled = CanSendMessage;
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

            int time = 1;
            if (mainWindow.data.messageCollection.Count > 0)
            {
                time += mainWindow.data.messageCollection.Last().Time;
            }

            WitMultiRangeSlider slider = editWindow.sliderGrid.Children.Cast<WitMultiRangeSlider>().First(s => Grid.GetRow(s) == Grid.GetRow(this));

            Message message = new Message(person, messageBox.Text, time, slider);
            editWindow.SetMessagePropertyChanged(message);

            mainWindow.data.messageCollection.Add(message);

            messageBox.Text = string.Empty;
        }
    }
}
