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

namespace ArtifactsMMO_Utility
{
    public partial class MainWindow : Window
    {
        HttpClient client =new HttpClient();
        private string server = "https://api.artifactsmmo.com";
        private string token;
        private string character; 
        public MainWindow()
        {
            InitializeComponent();
            token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VybmFtZSI6IlNwaXJpdGVjaG8iLCJwYXNzd29yZF9jaGFuZ2VkIjoiIn0.iqT-17qWcSH-dyIAQ-Nu-Fo8E7uzlUhsC7jay2_jDDs";
            character = "Asumi";
        }

        private async void Connect_Click (object sender, RoutedEventArgs e)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Envoyer une requête GET à l'URL
                HttpResponseMessage response = await client.GetAsync(server + "/client");

                // Vérifiez que la requête a réussi
                response.EnsureSuccessStatusCode();

                // Lire la réponse comme une chaîne
                string responseBody = await response.Content.ReadAsStringAsync();

                // Afficher la réponse dans une boîte de message ou mettre à jour l'interface utilisateur
                MessageBox.Show(responseBody);
            }
            catch (HttpRequestException ex)
            {
                // Afficher un message d'erreur en cas d'échec
                MessageBox.Show($"Erreur lors de la requête: {ex.Message}");
            }
        }

        private async void Fight_Click (object sender, RoutedEventArgs e)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Send POST request
                HttpResponseMessage response = await client.PostAsync(server + "/my/" + character + "/action/fight", null);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }

        private async void FightLoop_Click(object sender, RoutedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{server}/my/{character}/action/fight";

                // Configure headers
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                while (true)
                {
                    try
                    {
                        // Send POST request
                        HttpResponseMessage response = await client.PostAsync(url, null);

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                        {
                            Console.WriteLine("The character cannot be found on your account.");
                            return;
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                        {
                            Console.WriteLine("Your character's inventory is full.");
                            return;
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                        {
                            Console.WriteLine("Your character is in cooldown.");
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)598)
                        {
                            Console.WriteLine("No monster on this map.");
                            return;
                        }
                        else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine("An error occurred during the fight.");
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
                                string message = $"The fight ended successfully. You have {fightResult}.\n";
                                message += $"Cooldown: {cooldownSeconds} seconds";

                                // Afficher le message
                                Console.WriteLine(message, "Fight Result");

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
            }
        }

        private async void Recolt (object sender, RoutedEventArgs e)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Send POST request
                HttpResponseMessage response = await client.PostAsync(server + "/my/" + character + "/action/gathering", null);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException fe)
            {
                Console.WriteLine("Request error: " + fe.Message);
            }
        }

        private async void RecoltLoop(object sender, RoutedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{server}/my/{character}/action/gathering";

                // Configure headers
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                while (true)
                {
                    try
                    {
                        // Send POST request
                        HttpResponseMessage response = await client.PostAsync(url, null);

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 498
                        {
                            Console.WriteLine("The character cannot be found on your account.");
                            return;
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)497)
                        {
                            Console.WriteLine("Your character's inventory is full.");
                            return;
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)499)
                        {
                            Console.WriteLine("Your character is in cooldown.");
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)493)
                        {
                            Console.WriteLine("No resource on this map.");
                            return;
                        }
                        else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine("An error occurred while gathering the resource.");
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
                                string message = $"Your character successfully gathered the resource.'";
                                message += $"Cooldown: {cooldownSeconds} seconds";

                                // Afficher le message
                                Console.WriteLine(message, "Fight Result");

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
            }
        }
    }
}
