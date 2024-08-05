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
    public partial class CharacterCreation : Window
    {
        public string Skin;
        public CharacterCreation()
        {
            InitializeComponent();
        }

        private void ChangeSkin(object sender, SelectionChangedEventArgs e)
        {
            if (SkinList.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedSkin = selectedItem.Content.ToString();
                string imagePath = string.Empty;

                switch (selectedSkin)
                {
                    case "Men 1":
                        imagePath = "pack://application:,,,/Ressources/Skin/men1.png";
                        Skin = "men1";
                        break;
                    case "Men 2":
                        imagePath = "pack://application:,,,/Ressources/Skin/men2.png";
                        Skin = "men2";
                        break;
                    case "Men 3":
                        imagePath = "pack://application:,,,/Ressources/Skin/men3.png";
                        Skin = "men3";
                        break;
                    case "Women 1":
                        imagePath = "pack://application:,,,/Ressources/Skin/women1.png";
                        Skin = "women1";
                        break;
                    case "Women 2":
                        imagePath = "pack://application:,,,/Ressources/Skin/women2.png";
                        Skin = "women2";
                        break;
                    case "Women 3":
                        imagePath = "pack://application:,,,/Ressources/Skin/women3.png";
                        Skin = "women3";
                        break;
                    default:
                        imagePath = string.Empty;
                        Skin = string.Empty;
                        break;
                }

                if (!string.IsNullOrEmpty(imagePath))
                {
                    SkinPicture.Source = new BitmapImage(new Uri(imagePath));
                }
                else
                {
                    SkinPicture.Source = null;
                }
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PlayerName.Text) || string.IsNullOrEmpty(Skin))
            {
                MessageBox.Show("Please provide all required information.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else 
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
