# Detections

This document lists the five custom detection rules built for this lab. Each is
mapped to a MITRE ATT&CK technique, with the rule logic, the test used to
validate it, and the result.

## Summary

| Rule ID | Level | Detects | MITRE Technique | Status |
|---------|-------|---------|-----------------|--------|
| `100000` | 5  | Local account/group discovery via `net` | `T1087.001` | Validated |
| `100001` | 10 | Windows event log cleared via `wevtutil` | `T1070.001` | Validated |
| `100002` | 7  | Scheduled task creation | `T1053.005` | Validated |
| `100003` | 10 | SAM/SYSTEM hive dump via `reg.exe` | `T1003.002` | Redundant (built-in `92026`) |
| `100004` | 8  | Suspicious PowerShell execution | `T1059.001` | Validated |

> `100001` is tool-specific and can be evaded, you can see [EVASIONS.md](EVASIONS.md).
> The log-clearing *outcome* is caught independently by the built-in rule `63104`.

## Detection Details

### Rule `100000` - Local Account/Group Discovery
**MITRE:** `T1087.001` - Account Discovery: Local Account · **Level:** 5
**Log source:** Sysmon process creation (Event ID 1), chained off Wazuh rule `61603`
**Logic:** Fires when a process's parent is `net.exe` and the parent command line
contains `user`, `localgroup`, `group`, or `accounts` catching `net1.exe` spawned
by reconnaissance commands like `net user` or `net localgroup`.
**Test:**

Atomic Red Team T1087.001 (net user / Get-LocalUser).

**Result:**

Fired - rule 100000, level 5. evidence/p2-s7-first-alert.png

### Rule `100001` - Event Log Cleared via wevtutil
**MITRE:** `T1070.001` — Clear Windows Event Logs · **Level:** 10
**Log source:** Sysmon process creation (Event ID 1), chained off `61603`
**Logic:** Fires when the process image is `wevtutil.exe` and the command line
contains `cl` or `clear-log`. (Tool-specific — see EVASIONS.md.)
**Test:**

Ran `wevtutil cl` on WS01.

**Result:**

Fired - rule 100001, level 10. evidence/p2-s8-clear-logs-alert.png

### Rule `100002` - Scheduled Task Creation
**MITRE:** `T1053.005` — Scheduled Task · **Level:** 7
**Log source:** Sysmon file creation (Event ID 11), chained off Wazuh rule `61613`
**Logic:** Fires on a file write whose target filename contains `\Tasks`, catching
the task definition file dropped when a scheduled task is created.
**Test:** 

Atomic Red Team T1053.005 (scheduled task creation).

**Result:**

Fired - rule 100002, level 7. evidence/p2-s8-schtask-alert.png

### Rule `100003`- Credential Dumping (SAM/SYSTEM Hive)
**MITRE:** `T1003.002` — OS Credential Dumping: SAM · **Level:** 10
**Log source:** Sysmon process creation (Event ID 1), chained off `61603`
**Logic:** Fires when `reg.exe` (matched by `originalFileName`, so renaming it
doesn't help) runs with `save` and a hive name (`SAM`, `SYSTEM`, or `SECURITY`)
in the command line — the classic offline credential-theft move.
**Test:**

`reg save HKLM\sam` on WS01 (Atomic T1003.002).

**Result:**

Did not fire as custom rule - superseded by Wazuh built-in rule 92026 (level 14, T1003.002). evidence/p2-s8-sam-dump-builtin92026.png

### Rule `100004` - Suspicious PowerShell Execution
**MITRE:** `T1059.001` — PowerShell · **Level:** 8
**Log source:** Sysmon process creation (Event ID 1), chained off `61603`
**Logic:** Fires when PowerShell (matched by `originalFileName`, catching renames)
runs with credential-tooling or evasion indicators in the command line —
`Invoke-Mimikatz`, `DumpCreds`, `DownloadString`, `FromBase64String`, `IEX`,
`-enc`, `-nop`, or `hidden`.
**Test:**

Benign `powershell.exe -nop ...` (command-line indicator, T1059.001).

**Result:**

Fired - rule 100004, level 8. evidence/p2-s8-powershell-alert.png