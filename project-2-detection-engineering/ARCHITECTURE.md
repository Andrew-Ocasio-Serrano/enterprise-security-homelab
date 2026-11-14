# Architecture

This document describes the lab's components and how endpoint telemetry flows from the monitored Windows host (`WS01`) to the Wazuh dashboard.

## Components

| Host | Role | OS |
|------|------|-----|
| `wazuh-manager` | SIEM - manager, indexer, and dashboard | Ubuntu Server 26.04.1 |
| `WS01` | Monitored endpoint - Sysmon + Wazuh agent | Windows 11 |
| `kali` | Attacker - runs Atomic Red Team tests | Kali Linux |

All hosts run as VMware virtual machines. Internal IP addresses are intentionally
omitted from this repository.

## Data Flow
```
WS01 (Sysmon)
  └─► Windows Event Log
        └─► Wazuh Agent 
              └─► Wazuh Manager  (decoders + rules)
                    └─► Wazuh Indexer
                          └─► Dashboard
```

## Design Rationale

I use a SIEM because it gives a centralized place where I can see every log and also correlates every alert for me, all in the same place. Using Sysmon gives me more visibility since Sysmon runs as a Windows service with a kernel-mode driver, so it captures every low-level activity in the endpoint, providing rich, detailed telemetry. The attacker machine was a separate Kali machine to simulate a real-world attack, keeping the traffic distinct from the traffic I am monitoring. One limitation this lab has is that it has no network segmentation, removing an element of realism.