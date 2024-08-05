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

namespace ArtifactsMMO_Utility.Module
{
    public partial class Key_Information : Window
    {
        public Key_Information()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Token != null)
            {
                Info_Key.Text = Properties.Settings.Default.Token;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Token = Info_Key.Text;
            Properties.Settings.Default.Save();
            this.Hide();
        }
    }
}
