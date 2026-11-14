# WazuhAlertSummarizer

A C# (.NET 8) console tool that reads exported Wazuh alerts and prints a ranked
summary. Turning raw JSON alert logs into a quick triage view.

## What it does

Wazuh writes alerts as JSON (one object per line). This tool parses that file and
aggregates the alerts four ways:

- **By description** - which rules fired most
- **By severity (level)** - where the noise and the critical hits are
- **By agent** - which host generated the alerts
- **By MITRE technique** - which ATT&CK techniques were observed

It also prints the total alert count and the time range covered.

## Usage

    dotnet run -- <path-to-alerts.json>

Point it at an exported Wazuh `alerts.json` (JSON Lines, one alert per line).

## Example output

    Total alerts: 21
    Time range: 2026-11-09T21:55:28 → 2026-11-09T21:57:29

    === By Description ===
     17 Host-based anomaly detection event (rootcheck).
      1 Wazuh server started.
      ...
    === By Severity ===
     17 level 7
      4 level 3
    === By Agent ===
     21 wazuh-server
    === By Technique ===
      1 Valid Accounts
      1 Sudo and Sudo Caching

(evidence/p3-s10-summarizer-output.png)

## How it works

- **.NET 8** console app (LTS).
- Reads the file line by line and deserializes each with `System.Text.Json` into
  typed classes (`Alert`, `Rule`, `Mitre`, `Agent`).
- Aggregates with LINQ (`GroupBy` / `Select` / `OrderByDescending`).
- Timestamps are kept as strings to avoid parse errors on Wazuh's `+0000` offset format.

## Design note

I built this tool for two reasons. First, efficiency, instead of scrolling through thousands of alerts, 
it summarizes them all for me, and time is one of the most important things in cybersecurity. Second, 
to practice C# on a real security dataset, combining the skills I learned in the classroom with a real-world example.