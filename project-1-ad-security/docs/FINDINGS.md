## Finding 1: Nested Group Privilege Escalation to Domain Admin

**Affected accounts:** bocasio (Bob Ocasio), aalvarez (Andres Alvarez)

**Path:** Help Desk Support (group) → IT Admins (group) → Domain Admins (group)

**Root cause:** Help Desk Support was nested inside IT Admins, and IT
Admins was nested inside Domain Admins. Neither help desk user was
directly added to any admin group, so a flat review of Domain Admins'
direct membership would show nothing suspicious — the privilege only
becomes visible when group nesting is resolved recursively.

**Evidence:** `Get-ADGroupMember -Identity "Domain Admins" -Recursive`
returns bocasio and aalvarez alongside the built-in Administrator
account (see screenshot 12).

**MITRE ATT&CK mapping:** T1078.002 (Valid Accounts: Domain Accounts)

**Risk:** Any compromise of a help desk account — phishing, credential
reuse, weak password — results in immediate, undetected full domain
compromise, with no additional exploitation required.

**Remediation:** Remove "Help Desk Support" from "IT Admins." Apply
least-privilege group design: help desk roles should have scoped,
task-specific delegated permissions (e.g., password reset rights via
ACLs), not nested membership in privileged groups.


## Finding 2: Kerberoastable Service Account with Direct Domain Admin Membership

**Affected account:** svc-sql

**Root cause:** A service account was created with a Service Principal
Name (required for Kerberos-authenticated service communication), a
weak, non-expiring password, and direct membership in Domain Admins —
a common real-world shortcut to avoid service-account permission
troubleshooting.

**Evidence:** `setspn -L svc-sql` confirms the registered SPN
(MSSQLSvc/dc01.corp.local:1433). `Get-ADUser` confirms
PasswordNeverExpires is enabled and Domain Admins appears in MemberOf
(see screenshot 15).

**Attack path:** Any authenticated domain user — including a
low-privilege account like a Help Desk user — can request a Kerberos
service ticket for this SPN without any special privileges, then
attempt to crack the password offline (Kerberoasting). A successful
crack yields direct Domain Admin credentials.

**MITRE ATT&CK mapping:** T1558.003 (Steal or Forge Kerberos Tickets:
Kerberoasting), T1078.002 (Valid Accounts: Domain Accounts)

**Risk:** Unlike Finding 1 (which required tracing nested group
membership to discover), this account is *visibly* a Domain Admin —
the failure here is a lack of periodic privileged-account auditing,
combined with weak service account credential hygiene.

**Remediation:** Remove svc-sql from Domain Admins; apply a strong,
regularly rotated password or migrate to a Group Managed Service
Account (gMSA), which Windows can manage automatically without a
human-known password at all.

## Collection Methodology
Data collected via SharpHound v2.16.0, run as a standard, non-privileged
domain user (bocasio) from a domain-joined workstation (WS01) — not
from the domain controller — to accurately simulate what an attacker
with only basic user-level access could enumerate. Collection completed
in under 2 seconds, covering 315 AD objects, with zero access-denied
errors: notably, a completely unprivileged user was able to enumerate
the full domain structure (all users, groups, ACLs, and trust
relationships) using only default AD read permissions. This baseline
visibility is itself a real, discussable finding: default AD
configurations grant broad structural visibility to any authenticated
user, which is precisely what makes attack-path discovery via
BloodHound possible for real attackers with minimal initial access.

## Finding 3 (Discovered, Not Planted): GenericAll Rights via Account Operators

**Discovered organically** during BloodHound graph review — not an
intentionally planted misconfiguration like Findings 1 and 2.

**Path:** ACCOUNT OPERATORS (built-in group) → GenericAll → WS01.CORP.LOCAL

**Root cause:** The built-in "Account Operators" group holds GenericAll
(full control) rights over the WS01 computer object by default AD
behavior, not a deliberate lab misconfiguration.

**MITRE ATT&CK mapping:** T1098 (Account Manipulation)

**Risk:** Anyone with membership in Account Operators (a group often
under-scrutinized since it's not "Domain Admins") could fully
reconfigure or take control of WS01, including resetting its computer
account password to authenticate as the machine itself.

**Note:** This finding demonstrates the value of BloodHound over manual
review — this path was not intentionally built and was only surfaced
through graph analysis, mirroring how real assessments uncover
unexpected privilege relationships.