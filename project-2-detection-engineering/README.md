# Project 2 - Detection Engineering

I built a Windows detection lab using Wazuh and Sysmon, then tested my detections with Atomic Red Team and documented what was caught and what evaded them.

## Overview

This project deploys a Wazuh SIEM and onboards a Windows host running Sysmon for telemetry. Custom detection rules are written and mapped to MITRE ATT&CK. Each of the rules was validated with a controlled test, and selected evasions were run to expose the detection gaps.

## Objectives

- Deploy a Wazuh SIEM and onboard a Windows endpoint
- Collect endpoint telemetry using Sysmon 
- Write custom detection rules mapped to MITRE ATT&CK
- Validate each detection with a controlled test 
- Run evasions to find detection gaps
- Build a C# tool to summarize and triage alerts

## Lab Environment

| Component | Details |
|-----------|---------|
| SIEM | Wazuh 4.14.9-1 on Ubuntu Server 26.04.1 |
| Endpoint | Windows 11 - hostname `WS01`, Sysmon + Wazuh Agent |
| Attacker | Kali Linux |
| Hypervisor | VMware |

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full topology and data flow.

## Repository Contents

- **[ARCHITECTURE.md](ARCHITECTURE.md)** — lab topology and log flow
- **[DETECTIONS.md](DETECTIONS.md)** — each detection rule and its test
- **[EVASIONS.md](EVASIONS.md)** — what bypassed detections and how it was caught
- **[tools/](tools/)** — the C# alert-summarizer tool

## Key Outcomes

- Built 5 custom detections mapped to 5 MITRE ATT&CK techniques
- Demonstrated outcome-based detection caught log clearing by its result (`System` event 104, Wazuh rule `63104`) instead of the specific tool
- Documented 2 evasions, including a Windows event-log clearing technique that bypassed a tool-specific rule (keyed on `wevtutil`) but was still caught by the outcome-based rule `63104`
- Built a C# tool that turns raw Wazuh JSON alerts into a ranked triage view

## Skills Demonstrated

SIEM engineering, detection rule writing, MITRE ATT&CK mapping, Sysmon telemetry, adversary emulation, false-positive tuning, C# tooling