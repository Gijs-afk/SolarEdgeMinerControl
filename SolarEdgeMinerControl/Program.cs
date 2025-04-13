using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace SolarEdgeMinerControl
{

    class Program
    {
        // === CONFIGURE THESE ===
        static string solarEdgeApiKey = "YOUR_API_KEY_HERE";                            // Your SolarEdge API key
        static string solarEdgeSiteId = "YOUR_SITE_ID_HERE";                            // Your SolarEdge site ID
        static string watchDogAPIAuth = "YOUR_WATCHDOG_AUTHENTICATION_HERE";            // Your WatchDog API authentication token
        static int watchDogPort = 18000;                                                // Standard WatchDog port is 18000 change if different
        static int thresholdWatts = 600;                                                // Threshold in watts to start/stop the miner
        static int checkIntervalSeconds = 300;                                          // Check every 5 minutes

        static bool isMining = true;


        static async Task Main(string[] args)
        {
            Console.WriteLine("[INFO] SolarEdge Miner Control started");
            while (true)
            {
                try
                {
                    double power = await GetCurrentSolarPower();
                    Console.WriteLine($"[DATETIME]{DateTime.Now}             [INFO] Current solar output: {power}W");

                    if (power >= thresholdWatts)
                    {
                        if (!isMining)
                        {
                            await StartMiner();
                            isMining = true;
                        }
                        else
                        {
                            Console.WriteLine($"[DATETIME]{DateTime.Now}             [INFO] Miner is already running. No action taken");
                        }
                    }
                    else
                    {
                        if (isMining)
                        {
                            await StopMiner();
                            isMining = false;
                        }
                        else
                        {
                            Console.WriteLine($"[DATETIME]{DateTime.Now}             [INFO] Solar output is below threshold. No action taken");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("//////////////////////////////////////////////////////////////");
                    Console.WriteLine("");
                    Console.WriteLine($"[DATETIME]{DateTime.Now}             [ERROR] Failed to get solar power data: {ex.Message} ");
                    Console.WriteLine("");
                    Console.WriteLine("//////////////////////////////////////////////////////////////");
                    Console.WriteLine("");
                    Console.WriteLine("");
                }

                Thread.Sleep(checkIntervalSeconds * 1000);
            }
        }

        static async Task<double> GetCurrentSolarPower()
        {
            using HttpClient client = new HttpClient();
            string url = $"https://monitoringapi.solaredge.com/site/{solarEdgeSiteId}/overview.json?api_key={solarEdgeApiKey}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("overview").GetProperty("currentPower").GetProperty("power").GetDouble();
        }

        static async Task SendMinerCommand(string command)
        {
            using HttpClient client = new HttpClient();
            var commandJson = new
            {
                id = 1,
                method = command,
                @params = Array.Empty<object>()
            };

            string commandJsonString = JsonSerializer.Serialize(commandJson);
            string encodedCommand = Uri.EscapeDataString(commandJsonString);


            string url = $"http://localhost:{watchDogPort}/api?command={encodedCommand}";
            client.DefaultRequestHeaders.Add("Authorization", watchDogAPIAuth);

            try
            {
                var response = await client.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DATETIME]{DateTime.Now}             [ACTION] Send command: {command} : {response.StatusCode} - {body}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DATETIME]{DateTime.Now}             [ERROR] Failed to send command: {command} : {ex.Message}");
            }
        }

        static async Task StartMiner()
        {
            Console.WriteLine($"[DATETIME]{DateTime.Now}             [ACTION] Starting miner...");
            await SendMinerCommand("quit");
        }

        static async Task StopMiner()
        {
            Console.WriteLine($"[DATETIME]{DateTime.Now}             [ACTION] Stopping miner...");
            await SendMinerCommand("miner.stop");
        }
    }
}
