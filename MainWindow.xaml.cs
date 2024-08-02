using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using System.Text.Json;
using System.Windows.Markup;
using System.Text.Json.Nodes;
using System.Security.Policy;
using System.Runtime.InteropServices;

namespace ArtifactsMMO_Utility
{
    public partial class MainWindow : Window
    {
        #region Variable
        HttpClient client = new HttpClient();
        private Key_Information key_Information = new Key_Information();

        private string server = "https://api.artifactsmmo.com";
        private string token;
        private string character;
        
        private int PositionX;
        private int PositionY;
        private int price;

        private bool loopF = false;
        private bool isRedF = true;
        private bool loopR = false;
        private bool isRedR = true;
        private bool FFlag = false;
        private bool RFlag = false;

        private List<Player> player = new List<Player>();
        private List<string> itemList = new List<string>();
        private List<int> itemListCount = new List<int>();
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.Token != "")
            {
                token = Properties.Settings.Default.Token;
                ConfigureHttpClient();
                _ = Connect();
            }
            else
            {
                InfoKey();
            }
        }

        #region Connection Serveur
        private void InfoKey()
        {
            key_Information.ShowDialog();
            token = Properties.Settings.Default.Token;
            ConfigureHttpClient();
            _ = Connect();
        }

        private void ConfigureHttpClient()
        {
            System.Console.WriteLine(Properties.Settings.Default.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task Connect()
        {
            ListOfPlayer.Items.Clear();
            try
            {
                HttpResponseMessage response = await client.GetAsync(server + "/my/characters");
                response.EnsureSuccessStatusCode(); // Vérifie que la requête a réussi

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("account", out JsonElement accountElement))
                            {
                                Account_name.Content = accountElement.GetString();
                                break;
                            }
                        }
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("name", out JsonElement nameElement))
                            {
                                ListOfPlayer.Items.Add(nameElement.GetString());
                                player.Add(new Player
                                {
                                    PlayerNames = nameElement.GetString(),
                                    Task = "none"
                                });
                            }
                        }
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur lors de la requête: {ex.Message}");
            }
        }
        #endregion

        #region Action
        #region Compte
        private void Key_click(object sender, RoutedEventArgs e)
        {
            InfoKey();
        }

        private void Change_Player(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfPlayer.SelectedItem != null)
            {
                string selectedchanged = ListOfPlayer.SelectedItem.ToString();

                character = selectedchanged;
                ResetButtons();
            }
            else
            {
                character = null;
            }
        }
        #endregion

        #region Loop
        private async void FightLoop_Click(object sender, RoutedEventArgs e)
        {
            await Fight();
        }

        private async void RecolteLoop_Click(object sender, RoutedEventArgs e)
        {
           await Recolt();
        }
        #endregion

        private async void VenteAuto(string selectedPlayer)
        {
            #region Recuperation de position
            try
            {
                HttpResponseMessage response = await client.GetAsync(server + "/my/characters");
                response.EnsureSuccessStatusCode(); // Vérifie que la requête a réussi

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString() == selectedPlayer)
                            {
                                if (element.TryGetProperty("x", out JsonElement xElement))
                                {
                                    PositionX = xElement.GetInt32();
                                }

                                if (element.TryGetProperty("y", out JsonElement yElement))
                                {
                                    PositionY = yElement.GetInt32();
                                }

                                /*for (int i = 1; i < 20; i++)
                                {
                                    if (element.TryGetProperty($"inventory_slot{i}", out JsonElement itemElement))
                                    {
                                        itemList.Add(itemElement.GetString());
                                    }

                                    if (element.TryGetProperty($"inventory_slot{i}_quantity", out JsonElement quantityElement))
                                    {
                                        if (quantityElement.GetInt32() > 50) 
                                        { 
                                            itemListCount.Add(50);
                                        }
                                        else
                                        {
                                            itemListCount.Add(quantityElement.GetInt32());
                                        }
                                    }
                                }*/
                                break;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur lors de la requête: {ex.Message}");
            }
            #endregion

            await Move(5, 1, selectedPlayer);

            await FillItemList(selectedPlayer);

            await Vente(selectedPlayer);

            await Move(PositionX, PositionY,selectedPlayer);

            itemList.Clear();
            itemListCount.Clear();

            if (FFlag == true)
            {
                await Fight();
            }
            if (RFlag == true)
            {
                await Recolt();
            }
        }

        private void ResetButtons()
        {
            FightLoop.Background = new SolidColorBrush(Colors.Red);
            RecoltLoop.Background = new SolidColorBrush(Colors.Red);

            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    if (sPlayer.Task == "none")
                    {
                        FightLoop.Background = new SolidColorBrush(Colors.Red);
                        RecoltLoop.Background = new SolidColorBrush(Colors.Red);
                    }
                    if (sPlayer.Task == "combat")
                    {
                        FightLoop.Background = new SolidColorBrush(Colors.Green);
                        RecoltLoop.Background = new SolidColorBrush(Colors.Red);
                    }
                    if (sPlayer.Task == "recolt")
                    {
                        FightLoop.Background = new SolidColorBrush(Colors.Red);
                        RecoltLoop.Background = new SolidColorBrush(Colors.Green);
                    }
                }
            }
        }
        #endregion

        #region Task
        private async Task Fight()
        {
            string selectedPlayer = ListOfPlayer.SelectedItem.ToString();
            if (FFlag == true)
            {
                FFlag = false;
            }
            else
            {
                loopF = !loopF;
                FightLoop.Background = new SolidColorBrush(isRedF ? Colors.Green : Colors.Red);
                isRedF = !isRedF;
            }

            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "combat";
                }
            }

            string url = $"{server}/my/{selectedPlayer}/action/fight";

            while (loopF)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ($"The {selectedPlayer} cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ($"{selectedPlayer}'s inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            FFlag = true;                            
                            VenteAuto(selectedPlayer);
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ($"{selectedPlayer}'s is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)598)
                    {
                        Logs.Content = ($"{selectedPlayer}: No monster on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ($"{selectedPlayer}: An error occurred during the fight.");
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Raw JSON Response: " + responseBody);

                        // Lire le JSON directement
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            JsonElement root = doc.RootElement;

                            // Accéder aux données
                            var fightResult = root.GetProperty("data").GetProperty("fight").GetProperty("result").GetString();
                            var cooldownSeconds = root.GetProperty("data").GetProperty("cooldown").GetProperty("totalSeconds").GetInt32();

                            // Construire le message
                            string message = $"{selectedPlayer}: The fight ended successfully.";
                            message += $"Cooldown: {cooldownSeconds} seconds";

                            Logs.Content = (message);

                            // Attendre la fin du cooldown
                            await Task.Delay(cooldownSeconds * 1000);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException fe)
                {
                    Console.WriteLine("Request error: " + fe.Message);
                }
            }
            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "none";
                }
            }
            Logs.Content = $"{selectedPlayer} has stop";
        }

        private async Task Recolt()
        {
            string selectedPlayer = ListOfPlayer.SelectedItem.ToString();
            if (RFlag == true)
            {
                RFlag = false;
            }
            else
            {
                loopR = !loopR;
                RecoltLoop.Background = new SolidColorBrush(isRedR ? Colors.Green : Colors.Red);
                isRedR = !isRedR;
            }

            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "recolt";
                }
            }

            string url = $"{server}/my/{selectedPlayer}/action/gathering";

            while (loopR)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ($"The {selectedPlayer} cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ($"{selectedPlayer}'s inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            RFlag = true;
                            VenteAuto(selectedPlayer);
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ($"{selectedPlayer}'s is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)493)
                    {
                        Logs.Content = ($"{selectedPlayer}: No resource on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ($"{selectedPlayer}: An error occurred while gathering the resource.");
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Raw JSON Response: " + responseBody);

                        // Lire le JSON directement
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            JsonElement root = doc.RootElement;

                            // Accéder aux données
                            //var fightResult = root.GetProperty("data").GetProperty("fight").GetProperty("result").GetString();
                            var cooldownSeconds = root.GetProperty("data").GetProperty("cooldown").GetProperty("totalSeconds").GetInt32();

                            // Construire le message
                            string message = $"{selectedPlayer}'s successfully gathered the resource.'";
                            message += $"Cooldown: {cooldownSeconds} seconds";

                            Logs.Content = (message);

                            await Task.Delay(cooldownSeconds * 1000);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException fe)
                {
                    Console.WriteLine("Request error: " + fe.Message);
                }
            }
            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "none";
                }
            }
            Logs.Content = $"{selectedPlayer} has stop";
        }

        private async Task Transformation()
        {

        }

        private async Task FillItemList(string selectedPlayer)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(server + "/my/characters");
                response.EnsureSuccessStatusCode(); // Vérifie que la requête a réussi

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString() == selectedPlayer)
                            {
                                for (int i = 1; i < 20; i++)
                                {
                                    if (element.TryGetProperty($"inventory_slot{i}", out JsonElement itemElement))
                                    {
                                        itemList.Add(itemElement.GetString());
                                    }

                                    if (element.TryGetProperty($"inventory_slot{i}_quantity", out JsonElement quantityElement))
                                    {
                                        if (quantityElement.GetInt32() > 50)
                                        {
                                            itemListCount.Add(50);
                                        }
                                        else
                                        {
                                            itemListCount.Add(quantityElement.GetInt32());
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur lors de la requête: {ex.Message}");
            }
        }

        private async Task Vente(string selectedPlayer)
        {
            try
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (!string.IsNullOrEmpty(itemList[i]))
                    {
                        string url = $"{server}/items/{itemList[i]}";
                        HttpResponseMessage responseg = await client.GetAsync(url);
                        responseg.EnsureSuccessStatusCode();
                        string responseBodyg = await responseg.Content.ReadAsStringAsync();

                        using (JsonDocument doc = JsonDocument.Parse(responseBodyg))
                        {
                            JsonElement root = doc.RootElement;

                            if (root.TryGetProperty("data", out JsonElement geElement))
                            {
                                if (geElement.TryGetProperty("ge", out JsonElement ge2Element))
                                {
                                    if (ge2Element.TryGetProperty("sell_price", out JsonElement priceSold))
                                    {
                                        price = priceSold.GetInt32();

                                        url = $"{server}/my/{character}/action/ge/sell";
                                        string item = $"{{\"code\":\"{itemList[i]}\",\"quantity\":{itemListCount[i]},\"price\":{price}}}";
                                        var item2 = new StringContent(item, Encoding.UTF8, "application/json");                                    

                                        System.Console.WriteLine(item.ToString());
                                        try
                                        {
                                            HttpResponseMessage response = await client.PostAsync(url, item2);
                                            response.EnsureSuccessStatusCode();
                                            string responseBody = await response.Content.ReadAsStringAsync();
                                            Console.WriteLine(responseBody);

                                            using (JsonDocument doc2 = JsonDocument.Parse(responseBody))
                                            {
                                                JsonElement root2 = doc2.RootElement;

                                                var cooldownSeconds = root2.GetProperty("data").GetProperty("cooldown").GetProperty("totalSeconds").GetInt32();

                                                await Task.Delay(cooldownSeconds * 1000);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Request error: " + ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }

        private async Task Move(int x, int y, string selectedPlayer)
        {
            string url = $"{server}/my/{selectedPlayer}/action/move";
            var options = $"{{\n  \"x\": \"{x}\",\n  \"y\": \"{y}\"\n}}";

            var content = new StringContent(options, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            try
            {
                System.Console.WriteLine(requestMessage.Content);
                HttpResponseMessage response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                Logs.Content = $"{selectedPlayer} has move in {x},{y}";

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;

                    // Accéder aux données
                    var cooldownSeconds = root.GetProperty("data").GetProperty("cooldown").GetProperty("totalSeconds").GetInt32();

                    // Attendre la fin du cooldown
                    await Task.Delay(cooldownSeconds * 1000);
                }
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }
        #endregion
    }
}