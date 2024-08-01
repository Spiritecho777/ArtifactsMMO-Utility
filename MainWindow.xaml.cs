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
            //token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VybmFtZSI6IlNwaXJpdGVjaG8iLCJwYXNzd29yZF9jaGFuZ2VkIjoiIn0.iqT-17qWcSH-dyIAQ-Nu-Fo8E7uzlUhsC7jay2_jDDs";
        }

        private void Key_click(object sender, RoutedEventArgs e)
        {
            InfoKey();
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

        private void FightLoop_Click(object sender, RoutedEventArgs e)
        {
            FLoop();
        }

        private void RecolteLoop_Click(object sender, RoutedEventArgs e)
        {
            RLoop();
        }

        private async void VenteAuto()
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
                            if (element.TryGetProperty("x", out JsonElement accountElement))
                            {
                                PositionX = accountElement.GetInt32();
                                break;
                            }
                        }
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("y", out JsonElement accountElement))
                            {
                                PositionY = accountElement.GetInt32();
                                break;
                            }
                        }
                        foreach (JsonElement element in dataElement.EnumerateArray())
                        {
                            for (int i = 1; i < 20; i++)
                            {
                                if (element.TryGetProperty($"inventory_slot{i}", out JsonElement accountElement))
                                {
                                    itemList.Add(accountElement.ToString());                                
                                }
                                if (element.TryGetProperty($"inventory_slot{i}_quantity", out JsonElement accountElement2))
                                {
                                    itemListCount.Add(accountElement2.GetInt32());
                                }
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

            #region Déplacement
            string url = $"{server}/my/{character}/action/move";
            var options = new StringContent("{\n  \"x\": 5,\n  \"y\": 1\n}");

            string jsonBody = JsonSerializer.Serialize(options);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            try
            {
                HttpResponseMessage response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
            #endregion

            #region Vente
            url = $"{server}/my/{character}/action/ge/sell";
            try
            {
                for (int i =0; i < itemList.Count; i++)
                {
                    var item = new StringContent($"{{\"code\":\"{itemList[i]}\",\"quantity\":{itemListCount[i]},\"price\":1}}");
                    HttpResponseMessage response = await client.PostAsync(url, item);

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
            #endregion

            #region Retour
            url = $"{server}/my/{character}/action/move";
            options = new StringContent($"{{\n  \"x\": \"{PositionX}\",\n  \"y\": \"{PositionY}\"\n}}");

            jsonBody = JsonSerializer.Serialize(options);
            content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            try
            {
                HttpResponseMessage response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
            #endregion

            itemList.Clear();
            itemListCount.Clear();

            if (FFlag == true)
            {
                FFlag = false;
                FLoop();
            }
            if (RFlag == true)
            {
                RFlag = false;
                RLoop();
            }
        }

        private async void FLoop()
        {
            loopF = !loopF;
            FightLoop.Background = new SolidColorBrush(isRedF ? Colors.Green : Colors.Red);
            isRedF = !isRedF;

            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "combat";
                }
            }

            string url = $"{server}/my/{character}/action/fight";

            while (loopF)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ("The character cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ("Your character's inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            FFlag = true;
                            VenteAuto();
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ("Your character is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)598)
                    {
                        Logs.Content = ("No monster on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ("An error occurred during the fight.");
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
                            string message = $"The fight ended successfully.";
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
            Logs.Content = "You have stop";
        }

        private async void RLoop()
        {
            loopR = !loopR;
            RecoltLoop.Background = new SolidColorBrush(isRedR ? Colors.Green : Colors.Red);
            isRedR = !isRedR;

            foreach (var sPlayer in player)
            {
                if (ListOfPlayer.SelectedItem.ToString() == sPlayer.PlayerNames)
                {
                    sPlayer.Task = "recolt";
                }
            }

            string url = $"{server}/my/{character}/action/gathering";

            while (loopR)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                    {
                        Logs.Content = ("The character cannot be found on your account.");
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                    {
                        Logs.Content = ("Your character's inventory is full.");
                        if (AutoCheck.IsChecked == true)
                        {
                            RFlag = true;
                            VenteAuto();
                        }
                        return;
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                    {
                        Logs.Content = ("Your character is in cooldown.");
                    }
                    else if (response.StatusCode == (System.Net.HttpStatusCode)493)
                    {
                        Logs.Content = ("No resource on this map.");
                        return;
                    }
                    else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logs.Content = ("An error occurred while gathering the resource.");
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
                            string message = $"Your character successfully gathered the resource.'";
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
            Logs.Content = "You have stop";
        }
    }
}

/*
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                JsonElement root = doc.RootElement;

                // Accéder aux données
                var cooldownSeconds = root.GetProperty("data").GetProperty("cooldown").GetProperty("totalSeconds").GetInt32();

                // Attendre la fin du cooldown
                await Task.Delay(cooldownSeconds * 1000);
                }
*/