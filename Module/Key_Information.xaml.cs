using System.Windows;

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
