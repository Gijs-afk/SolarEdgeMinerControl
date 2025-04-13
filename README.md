# ☀️ SolarEdge Miner Control

This is a lightweight C# console application that automates GPU mining (via NiceHash QuickMiner) based on your real-time **solar power production** using the SolarEdge API.

When solar output exceeds a configured threshold (e.g., 300W), the app starts your miner. When production drops below the threshold, it stops mining—saving electricity and maximizing your ROI when using solar.

---

## ⚙️ Configuration

In `Program.cs`, set the following values:

```csharp
static string apiKey = "YOUR_API_KEY_HERE";             // Your SolarEdge API key
static string siteId = "YOUR_SITE_ID_HERE";             // Your SolarEdge site ID
static int thresholdWatts = 300;                        // Minimum solar output to start mining
static string minerProcessName = "nhqm";                // QuickMiner process name (usually 'nhqm')
static string minerPath = @"C:\\Program Files\\NiceHash QuickMiner\\nhqm.exe"; // Path to QuickMiner EXE
``` 
## 🛠 How to Compile to .EXE (using .NET 9.0)
Make sure you have the .NET 9.0 SDK installed. Then:

Option A: Using Visual Studio
Open the solution in Visual Studio 2022+

Set configuration to Release

Build → Build Solution

The .exe will be output to:


``` python
bin\Release\net9.0\SolarEdgeMinerControl.exe
```
Option B: Using the Command Line
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
This produces a standalone .exe that includes everything needed to run—no .NET runtime required.

You’ll find the .exe in:

```python
bin\Release\net9.0\win-x64\publish\
```

## 🔁 Set It to Run on Windows Startup
#### 🧩 Option 1: Startup Folder (Simple)
Press Win + R, type **shell:startup**, hit Enter

Copy a shortcut to the .exe into this folder

It will now run every time you log into Windows

#### ⚙️ Option 2: Task Scheduler (Advanced)
Open Task Scheduler → Create Basic Task

Name it SolarEdge Miner Control

Trigger: When I log on

Action: Start a program → browse to your .exe

Optional: check “Run with highest privileges”

## ✅ Requirements
Windows 10/11 (64-bit)

.NET 9.0 SDK (for development)

SolarEdge API access (API Key + Site ID)

NiceHash QuickMiner installed and tested

## 🧠 Notes
This app polls SolarEdge every 5 minutes and reacts to real-time data

The miner only starts when you have enough solar production





