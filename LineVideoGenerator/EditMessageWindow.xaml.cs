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
