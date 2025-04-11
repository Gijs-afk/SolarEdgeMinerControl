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
        static string apiKey = "YOUR_API_KEY_HERE";
        static string siteId = "YOUR_SITE_ID_HERE";
        static int thresholdWatts = 300; // Start mining if solar production > 300W
        static int checkIntervalSeconds = 300; // Check every 5 minutes
        static string minerProcessName = "nhqm"; // NiceHash QuickMiner process name
        static string minerPath = @"C:\\Program Files\\NiceHash QuickMiner\\nhqm.exe"; // Adjust if different

        static async Task Main(string[] args)
        {
            Console.WriteLine("[INFO] SolarEdge Miner Control started");
            while (true)
            {
                try
                {
                    double power = await GetCurrentSolarPower();
                    Console.WriteLine($"[INFO] Current solar output: {power}W");

                    if (power >= thresholdWatts)
                    {
                        StartMiner();
                    }
                    else
                    {
                        StopMiner();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                }

                Thread.Sleep(checkIntervalSeconds * 1000);
            }
        }

        static async Task<double> GetCurrentSolarPower()
        {
            using HttpClient client = new HttpClient();
            string url = $"https://monitoringapi.solaredge.com/site/{siteId}/overview.json?api_key={apiKey}";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("overview").GetProperty("currentPower").GetProperty("power").GetDouble();
        }

        static void StartMiner()
        {
            if (!Process.GetProcessesByName(minerProcessName).Any())
            {
                Console.WriteLine("[ACTION] Starting miner...");
                Process.Start(minerPath);
            }
        }

        static void StopMiner()
        {
            foreach (var proc in Process.GetProcessesByName(minerProcessName))
            {
                Console.WriteLine("[ACTION] Stopping miner...");
                proc.Kill();
            }
        }
    }
}