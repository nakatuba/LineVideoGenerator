using InWit.WPF.MultiRangeSlider;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LineVideoGenerator
{
    /// <summary>
    /// SendMyMessageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SendMyMessageControl : UserControl
    {
        public SendMyMessageControl()
        {
            InitializeComponent();
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            sendButton.IsEnabled = !string.IsNullOrWhiteSpace(messageBox.Text);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = Window.GetWindow(this) as EditWindow;
            MainWindow mainWindow = editWindow.Owner as MainWindow;

            double time = 1;
            if (mainWindow.data.messageCollection.Count > 0)
            {
                time += mainWindow.data.messageCollection.Last().Time;
            }

            WitMultiRangeSlider slider = editWindow.sliderGrid.Children.Cast<UIElement>().First(e2 => Grid.GetRow(e2) == Grid.GetRow(this)) as WitMultiRangeSlider;

            Message message = new Message(messageBox.Text, time, slider);

            Binding binding = new Binding("Time") { Source = message };
            message.sliderItem.SetBinding(RangeBase.ValueProperty, binding);

            message.PropertyChanged += (sender2, e2) =>
            {
                mainWindow.saveButton.IsEnabled = false;
                editWindow.dataGrid.Items.Refresh();
            };

            mainWindow.data.messageCollection.Add(message);
            mainWindow.SendMessage(message);
            mainWindow.playButton.IsEnabled = true;

            messageBox.Text = string.Empty;
        }
    }
}
