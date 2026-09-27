# Project 1 — Enterprise AD Security

## Objective

Build a realistic Active Directory environment, use BloodHound to
discover real privilege-escalation attack paths, remediate them, and
retest to confirm each fix actually worked — documented end to end as
a professional security assessment would be.

## Environment

- **Domain Controller (DC01):** Windows Server 2022 Standard (Desktop
  Experience), VMware Workstation, 4GB RAM / 2 vCPU
- **Workstation (WS01):** Windows 11 Enterprise Evaluation, domain-joined,
  used for realistic (non-DC, non-admin) SharpHound collection
- **Attacker/Collector (Kali):** Official Kali Linux VMware image,
  running BloodHound Community Edition via Docker
- **Domain:** corp.local, ~20 objects (10 users across IT, Finance, HR,
  Executives OUs, plus groups and a service account)

Full environment details: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Methodology

1. Build a realistic AD environment with both intentional and
   naturally occurring misconfigurations
2. Collect AD relationship data with SharpHound, run as a standard,
   non-privileged domain user, not from the domain controller, to
   simulate a real attacker's starting position
3. Analyze the resulting graph in BloodHound to discover attack paths
4. Remediate each finding at the directory level
5. Retest via a fresh collection to confirm each fix actually closed
   the path, not just that a command ran without error

## Findings Summary

| # | Finding | Type | Status |
|---|---|---|---|
| 1 | Nested group privilege escalation (Help Desk Support → IT Admins → Domain Admins) | Intentionally planted | Remediated & retested |
| 2 | Kerberoastable service account with direct Domain Admin membership (svc-sql) | Intentionally planted | Remediated & retested |
| 3 | GenericAll rights via built-in Account Operators group over WS01 | Discovered organically | Remediated & retested |
| 4 | SQLAdmin relationship surviving Finding 2's group membership fix (SPN-based) | Discovered during retest | Remediated & retested |

Full detail: [docs/FINDINGS.md](docs/FINDINGS.md)
Remediation record: [remediation/REMEDIATION.md](remediation/REMEDIATION.md)

## Screenshots

See [docs/screenshots/](docs/screenshots/) 32 screenshots covering
the complete build, from project scaffolding through the final
fully-remediated attack path graph. Screenshot 25 (initial vulnerable
state) and Screenshot 32 (final remediated state) are directly
comparable before/after evidence.

## Key Takeaway

Screenshot 29 is arguably the most important evidence in this project:
remediating Finding 2's obvious group membership did **not** fully
close the account's path to compromise. a separate SPN-based
`SQLAdmin` relationship remained, surfaced only through a fresh
BloodHound retest. This demonstrates why retesting via full data
re-collection, not just spot-checking the specific thing you fixed, is
essential in real security work.

## Lessons Learned

See the bottom of [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the
full write-up of technical lessons from this build.