using System.Windows;
using System.Windows.Controls;

namespace LineVideoGenerator
{
    /// <summary>
    /// EditMessageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditMessageWindow : Window
    {
        private Message message;

        public EditMessageWindow(Message message)
        {
            InitializeComponent();
            this.message = message;
            messageBox.Text = message.Text;
            changeButton.IsEnabled = false;
        }

        private void MessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeButton.IsEnabled = true;
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            message.Text = messageBox.Text;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
