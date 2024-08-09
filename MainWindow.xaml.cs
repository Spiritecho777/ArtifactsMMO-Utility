using ArtifactsMMO_Utility.Module;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;

namespace ArtifactsMMO_Utility
{
    public partial class MainWindow : Window
    {
        #region Variable
        HttpClient client = new HttpClient();
        private Player _currentPlayer;
        private Key_Information key_Information = new Key_Information();

        private string server = "https://api.artifactsmmo.com";
        private string token;
        private string character;
        private string selectedPlayer;
        
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
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

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
                Console.WriteLine($"Erreur lors de la requête: {ex.Message}");
                await CreationCharacterTask();
            }
        }
        #endregion

        #region Action
        #region Compte
        private void Key_click(object sender, RoutedEventArgs e)
        {
            InfoKey();
        }

        private async void Change_Player(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfPlayer.SelectedItem != null)
            {
                string selectedchanged = ListOfPlayer.SelectedItem.ToString();

                character = selectedchanged;

                Player currentPlayer = null;
                foreach (var sPlayer in this.player)
                {
                    if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                    {
                        currentPlayer = sPlayer;
                        break;
                    }
                }

                if (_currentPlayer != null)
                {
                    _currentPlayer.AutoSellCheck = AutoCheck.IsChecked ?? false;
                }

                _currentPlayer = currentPlayer;

                if (_currentPlayer != null)
                {
                    AutoCheck.IsChecked = _currentPlayer.AutoSellCheck;
                }

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
                                if (element.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString() == character)
                                {
                                    if (element.TryGetProperty("skin", out JsonElement xElement))
                                    {
                                        string Skin = xElement.ToString();
                                        string imagePath = $"pack://application:,,,/Ressources/Skin/{Skin}.png";
                                        SkinPicture.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                                        SkinPicture.UpdateLayout();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    ResetButtons();
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Erreur lors de la requête: {ex.Message}");
                }
            }
            else
            {
                character = null;
            }
        }
        #endregion

        #region Bouton
        private async void FightLoop_Click(object sender, RoutedEventArgs e)
        {
            Player selectedPlayer = null;
            foreach (var sPlayer in this.player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    selectedPlayer = sPlayer;
                    break;
                }
            }
            if (selectedPlayer != null)
            {
                await Fight(selectedPlayer);
            }
            else
            {
                MessageBox.Show("No matching player found.");
            }
        }

        private async void RecolteLoop_Click(object sender, RoutedEventArgs e)
        {
            Player selectedPlayer = null;
            foreach (var sPlayer in this.player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    selectedPlayer = sPlayer;
                    break;
                }
            }
            if (selectedPlayer != null)
            {
                await Recolt(selectedPlayer);
            }
            else
            {
                MessageBox.Show("No matching player found.");
            }
        }

        private async void CharacterCreation_Click(object sender, RoutedEventArgs e)
        {
            await CreationCharacterTask();
        }
        #endregion

        private async void VenteAuto(Player selectedPlayer)
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
                            if (element.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString() == selectedPlayer.PlayerNames)
                            {
                                if (element.TryGetProperty("x", out JsonElement xElement))
                                {
                                    selectedPlayer.PositionX = xElement.GetInt32();
                                }

                                if (element.TryGetProperty("y", out JsonElement yElement))
                                {
                                    selectedPlayer.PositionY = yElement.GetInt32();
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
            #endregion

            await Move(5, 1, selectedPlayer.PlayerNames);

            await FillItemList(selectedPlayer.PlayerNames);

            await Vente(selectedPlayer.PlayerNames);

            await Move(selectedPlayer.PositionX, selectedPlayer.PositionY,selectedPlayer.PlayerNames);

            itemList.Clear();
            itemListCount.Clear();

            if (selectedPlayer.FFlag == true)
            {
                selectedPlayer.FFlag = false;
                await Fight(selectedPlayer);
            }
            if (selectedPlayer.RFlag == true)
            {
                selectedPlayer.RFlag = false;
                await Recolt(selectedPlayer);
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
                    switch (sPlayer.Task)
                    {
                        case "none":
                            FightLoop.Background = new SolidColorBrush(Colors.Red);
                            RecoltLoop.Background = new SolidColorBrush(Colors.Red);
                            break;
                        case "combat":
                            FightLoop.Background = new SolidColorBrush(Colors.Green);
                            RecoltLoop.Background = new SolidColorBrush(Colors.Red);
                            break;
                        case "recolt":
                            FightLoop.Background = new SolidColorBrush(Colors.Red);
                            RecoltLoop.Background = new SolidColorBrush(Colors.Green);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Task
        private async Task Fight(Player player)
        {
            int i = 1;
            if (player.FFlag)
            {
                player.FFlag = false;
                player.Task = "none";
                ResetButtons();
                return;
            }
            else
            {
                player.FFlag = true;
                player.Task = "combat";
                ResetButtons();
            }

            string url = $"{server}/my/{player.PlayerNames}/action/fight";
            Console.WriteLine(url);
            while (player.FFlag)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ($"The {player.PlayerNames} cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ($"{player.PlayerNames}'s inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            FFlag = true;                            
                            VenteAuto(player);
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ($"{player.PlayerNames}'s is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)598)
                    {
                        Logs.Content = ($"{player.PlayerNames}: No monster on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ($"{player.PlayerNames}: An error occurred during the fight.");
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Raw JSON Response: " + responseBody);
                        Logs.Content = $"{player.PlayerNames} has won ({i})";
                        i++;
                        // Lire le JSON directement
                        await Cooldown(responseBody);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        player.FFlag = false;
                        player.Task = "none";
                        ResetButtons();
                    }
                }
                catch (HttpRequestException fe)
                {
                    Console.WriteLine("Request error: " + fe.Message);
                    player.FFlag = false;
                    player.Task = "none";
                    ResetButtons();
                }
            }
            Logs.Content = $"{player.PlayerNames} has stop";
        }

        private async Task Recolt(Player player)
        {
            int i = 1;
            if (player.RFlag)
            {
                player.RFlag = false;
                player.Task = "none";
                ResetButtons();
                return;
            }
            else
            {
                player.RFlag = true;
                player.Task = "recolt";
                ResetButtons();
            }

            string url = $"{server}/my/{player.PlayerNames}/action/gathering";

            while (player.RFlag)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ($"The {player.PlayerNames} cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ($"{player.PlayerNames}'s inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            RFlag = true;
                            VenteAuto(player);
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ($"{player.PlayerNames}'s is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)493)
                    {
                        Logs.Content = ($"{player.PlayerNames}: No resource on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ($"{player.PlayerNames}: An error occurred while gathering the resource.");
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Raw JSON Response: " + responseBody);
                        Logs.Content = $"{player.PlayerNames} has gather ({i})";
                        i++;
                        // Lire le JSON directement
                        await Cooldown(responseBody);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        player.RFlag = false;
                        player.Task = "none";
                        ResetButtons();
                    }
                }
                catch (HttpRequestException fe)
                {
                    Console.WriteLine("Request error: " + fe.Message);
                    player.RFlag = false;
                    player.Task = "none";
                    ResetButtons();
                }
            }
            Logs.Content = $"{player.PlayerNames} has stop";
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
                                if (element.TryGetProperty("inventory", out JsonElement inventoryElement) && inventoryElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement slotElement in inventoryElement.EnumerateArray())
                                    {
                                        if (slotElement.TryGetProperty("code", out JsonElement codeElement) &&
                                            slotElement.TryGetProperty("quantity", out JsonElement quantityElement))
                                        {
                                            string code = codeElement.GetString();
                                            int quantity = quantityElement.GetInt32();

                                            if (!string.IsNullOrEmpty(code))
                                            {
                                                itemList.Add(code);
                                            }

                                            if (quantity > 0)
                                            {
                                                itemListCount.Add(quantity > 50 ? 50 : quantity);
                                            }

                                            Console.WriteLine($"Item: {code}, Quantity: {quantity}");
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

                                        url = $"{server}/my/{selectedPlayer}/action/ge/sell";
                                        string item = $"{{\"code\":\"{itemList[i]}\",\"quantity\":{itemListCount[i]},\"price\":{price}}}";
                                        var item2 = new StringContent(item, Encoding.UTF8, "application/json");                                    

                                        System.Console.WriteLine(item.ToString());
                                        try
                                        {
                                            HttpResponseMessage response = await client.PostAsync(url, item2);
                                            response.EnsureSuccessStatusCode();
                                            string responseBody = await response.Content.ReadAsStringAsync();
                                            Console.WriteLine(responseBody);

                                            await Cooldown(responseBody);
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

                await Cooldown(responseBody);
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }

        private async Task CreationCharacterTask()
        {
            CharacterCreation characterCreation = new CharacterCreation();
            bool? result = characterCreation.ShowDialog();
            if (result == true)
            {
                string newCharacterName = characterCreation.PlayerName.Text;
                string newCharacterSkin = characterCreation.Skin;

                string url = $"{server}/characters/create";
                var options = $"{{\n  \"name\": \"{newCharacterName}\",\n  \"skin\": \"{newCharacterSkin}\"\n}}";
                Console.WriteLine(options);
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
                }
                catch (HttpRequestException fe)
                {
                    Console.WriteLine("Request error: " + fe.Message);
                }
                //characterCreation.DialogResult = false;
                await Connect();
            }
            else
            {
                //_ = Connect();
            }
        }

        private async Task Cooldown(string responseBody)
        {
            try 
            { 
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;

                    // Accéder aux données
                    var cooldownSeconds = root.GetProperty("data").GetProperty("cooldown").GetProperty("total_seconds").GetInt32();

                    // Attendre la fin du cooldown
                    await Task.Delay(cooldownSeconds * 1000);
                }
            }catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }
        #endregion

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Terminer l'application
            Application.Current.Shutdown();
        }

        /*private async void Map_Click(object sender, RoutedEventArgs e)
        {
            int page = 1;
            int pageSize = 50; // Définir la taille de la page selon les capacités de l'API
            bool hasMoreData = true;
            List<JsonElement> allMapElements = new List<JsonElement>();

            while (hasMoreData)
            {
                string url = $"{server}/maps?page={page}&pageSize={pageSize}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(body);

                    // Parse the JSON response
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        JsonElement root = doc.RootElement;

                        if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            var elements = dataElement.EnumerateArray().ToList();
                            if (elements.Count < pageSize)
                            {
                                hasMoreData = false;
                            }

                            // Filter out elements where content is null or empty
                            var filteredElements = elements
                                .Where(element =>
                                {
                                    // Check for the presence and type of "content"
                                    if (element.TryGetProperty("content", out JsonElement contentElement))
                                    {
                                        if (contentElement.ValueKind == JsonValueKind.String)
                                        {
                                            string content = contentElement.GetString();
                                            return !string.IsNullOrWhiteSpace(content);
                                        }
                                        return false; // content is not a string or is null
                                    }
                                    return false; // "content" property is missing
                                })
                                .ToList();

                            allMapElements.AddRange(filteredElements);
                        }
                        else
                        {
                            hasMoreData = false; // Arrêter la boucle si on ne trouve pas de "data" ou si ce n'est pas un array
                        }
                    }
                }
                page++;
            }

            // Process all collected map elements
            foreach (var element in allMapElements)
            {
                string name = element.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : "Unknown";
                string skin = element.TryGetProperty("skin", out JsonElement skinElement) ? skinElement.GetString() : "Unknown";
                int x = element.TryGetProperty("x", out JsonElement xElement) ? xElement.GetInt32() : 0;
                int y = element.TryGetProperty("y", out JsonElement yElement) ? yElement.GetInt32() : 0;
                string content = element.TryGetProperty("content", out JsonElement contentElement) && contentElement.ValueKind == JsonValueKind.String ? contentElement.GetString() : null;

                if (!string.IsNullOrEmpty(content))
                {
                    // Only process elements where content is not null or empty
                    Console.WriteLine($"Name: {name}, Skin: {skin}, X: {x}, Y: {y}, Content: {content}");
                }
            }
        }*/
    }
}