# Evasions

After validating the detections, selected attacks were re-run in an evaded form
a variation designed to avoid the custom rule. With the purpose to find where detection fails and
to compare tool-specific rules against outcome-based detection.

## Evasion 1 - Local Account Discovery via PowerShell (uncaught)

**Target rule:** `100000` - keys on `net.exe` (detects `net user` / `net localgroup`
account discovery, MITRE T1087.001).

**Evasion:** The same reconnaissance - enumerating local accounts was performed
with the PowerShell cmdlet `Get-LocalUser` instead of the `net` command.
(evidence/p2-s9-evasion1-action.png)

**Why it evaded:** `Get-LocalUser` is native PowerShell and never invokes `net.exe`,
so none of rule `100000`'s conditions matched. The attacker's *goal* (local account
discovery) was achieved, but the *tool* the rule watches for was never used.
(evidence/p2-s9-evasion1-blind.png)

**Result:** No alert - the activity went completely undetected, by the custom rule
and the built-in ruleset alike. (evidence/p2-s9-evasion1-noalert.png)

**Lesson:** Same weakness as Evasion 2 (tool-specific detection) but with no safety
net. Rule `100000` watches for `net.exe`; an attacker enumerating accounts any other
way `Get-LocalUser`, WMI, direct API slips past. This is an open detection gap.
The fix is behavior-based: detect the enumeration by its effect, or add coverage for
the PowerShell cmdlets, not just `net.exe`.

## Evasion 2 - Log Clearing via PowerShell

**Target rule:** `100001` - keys on the process image `wevtutil.exe`.

**Evasion:** Instead of `wevtutil cl`, the event log was cleared with PowerShell's
`Clear-EventLog`. Because that never invokes `wevtutil.exe`, rule `100001` never
matched, the custom detection went blind.
(evidence/p2-s9-evasion2-action.png)

**But it was still caught** - by Wazuh's built-in rule `63104`, which fires on
System **Event ID 104** ("the log file was cleared"), written by Windows regardless
of the tool used.
(evidence/p2-s9-evasion2-detected-builtin63104.png)

**Lesson:** Tool-based vs. outcome-based detection. Rule `100001` keys on *how*
(a specific binary) and is trivially evaded by switching tools. Rule `63104` keys
on *what happened* (the log was cleared) and catches every variant.

## Conclusion

The two evasions I did had the same flaw: the rules were created based on specific tools.
Outcome-based detection (like rule 63104) caught one; the other,with no behavioral backstop, was missed entirely.
The biggest lesson I learn doing this project is to detect behavioral over tools.