# Findings

## Collection Methodology

Data collected via SharpHound v2.16.0, run as a standard, non-
privileged domain user (bocasio) from a domain-joined workstation
(WS01) not from the domain controller. See
[ARCHITECTURE.md](ARCHITECTURE.md) for full methodology notes.

---

## Finding 1: Nested Group Privilege Escalation to Domain Admin

**Status: Remediated & retested**

**Affected accounts:** bocasio (Bob Ocasio), aalvarez (Andres Alvarez)

**Path:** Help Desk Support (group) → IT Admins (group) → Domain
Admins (group)

**Root cause:** Help Desk Support was nested inside IT Admins, and IT
Admins was nested inside Domain Admins. Neither help desk user was
directly added to any admin group, so a flat review of Domain Admins'
direct membership would show nothing suspicious, the privilege only
becomes visible when group nesting is resolved recursively.

**Evidence:** `Get-ADGroupMember -Identity "Domain Admins" -Recursive`
returned bocasio and aalvarez alongside the built-in Administrator
account (screenshot 12). Visually confirmed in BloodHound (screenshot
25).

**MITRE ATT&CK mapping:** T1078.002 (Valid Accounts: Domain Accounts)

**Risk:** Any compromise of a help desk account results in immediate,
undetected full domain compromise, with no additional exploitation
required.

**Remediation applied:**
```powershell
Remove-ADGroupMember -Identity "IT Admins" -Members "Help Desk Support" -Confirm:$false
```
Verified via `Get-ADGroupMember -Recursive`, now returning only the
built-in Administrator account (screenshot 26).

**Retest confirmation:** Confirmed via fresh SharpHound collection and
BloodHound re-import (database cleared before re-upload). The
AALVAREZ/BOCASIO → Help Desk Support → IT Admins → Domain Admins path
no longer appears in the graph. See screenshot 27, directly comparable
to screenshot 25.

---

## Finding 2: Kerberoastable Service Account with Direct Domain Admin Membership

**Status: Remediated & retested**

**Affected account:** svc-sql

**Root cause:** A service account was created with a Service Principal
Name (MSSQLSvc/dc01.corp.local:1433), a weak, non-expiring password,
and direct membership in Domain Admins. A common real-world shortcut
to avoid service-account permission troubleshooting.

**Evidence:** `setspn -L svc-sql` confirmed the registered SPN.
`Get-ADUser` confirmed PasswordNeverExpires was enabled and Domain
Admins appeared in MemberOf (screenshot 15).

**Attack path:** Any authenticated domain user could request a Kerberos
service ticket for this SPN without special privileges, then attempt
to crack the password offline (Kerberoasting). A successful crack
yields direct Domain Admin credentials.

**MITRE ATT&CK mapping:** T1558.003 (Kerberoasting), T1078.002 (Valid
Accounts: Domain Accounts)

**Risk:** Unlike Finding 1, this account was *visibly* a Domain Admin. 
The failure here was a lack of periodic privileged-account auditing
combined with weak service-account credential hygiene.

**Remediation applied:**
```powershell
Remove-ADGroupMember -Identity "Domain Admins" -Members "svc-sql" -Confirm:$false
Set-ADAccountPassword -Identity "svc-sql" -Reset -NewPassword (secure string)
```
Group membership removed and password rotated to a strong value.
Verified via `Get-ADGroupMember -Recursive`, now returning only the
built-in Administrator account (screenshot 28).

**Retest confirmation:** The direct svc-sql → Domain Admins membership
edge no longer appears in the graph (screenshot 29) However, this
retest surfaced a **separate, previously-hidden path**, documented
below as Finding 4.

---

## Finding 3 (Discovered, Not Planted): GenericAll Rights via Account Operators

**Status: Remediated & retested**

**Discovered organically** during initial BloodHound graph review
(screenshot 25) not an intentionally planted misconfiguration.

**Path:** ACCOUNT OPERATORS (built-in group) → GenericAll → WS01.CORP.LOCAL

**Root cause:** The built-in "Account Operators" group holds GenericAll
(full control) rights over the WS01 computer object by default Active
Directory behavior. Group membership was confirmed empty
(`Get-ADGroupMember` returned no results) this finding reflects a
latent default-permission risk, not active exploitation via existing
membership.

**MITRE ATT&CK mapping:** T1098 (Account Manipulation), T1484 (Domain
Policy Modification)

**Risk:** Anyone granted membership in Account Operators in the future. 
A group often under-scrutinized since it isn't "Domain Admins"
could fully reconfigure or take control of WS01, including resetting
its computer account password.

**Note on discovery method:** This finding demonstrates the core value
of graph-based analysis over manual review, it was not intentionally
built into the lab and was only surfaced through BloodHound's
automated path analysis.

**Remediation applied:**
```powershell
$acl.RemoveAccessRule($aceToRemove)
Set-Acl "AD:\$($ws01.DistinguishedName)" -AclObject $acl
```
Removed the GenericAll ACE held by BUILTIN\Account Operators over
WS01's computer object. `RemoveAccessRule` returned `True`, confirming
a matching rule was found and removed. Verified via a follow-up
`Get-Acl` query, which returned no Account Operators entry at all
(screenshot 30).

**Retest confirmation:** Confirmed via final combined retest
(screenshot 32) no GenericAll relationship from Account Operators to
WS01 remains.

---

## Finding 4 (Discovered via Retest, Not Planted): SQLAdmin Relationship Surviving Group-Membership Remediation

**Status: Remediated & retested**

**Discovered during retest of Finding 2** (screenshot 29) is not
visible in earlier graphs because it was overshadowed by the more
obvious direct Domain Admin membership edge. Removing that membership
did not affect this separate relationship.

**Path:** SVC-SQL@CORP.LOCAL → SQLAdmin → DC01.CORP.LOCAL

**Root cause:** The SPN registered as part of Finding 2's setup
(MSSQLSvc/dc01.corp.local:1433) causes BloodHound to infer that
svc-sql is the service account for a SQL Server instance running
directly on the domain controller, granting SQL sysadmin-equivalent
rights on DC01 itself, independent of any AD group membership. Per
BloodHound's own relationship documentation: an MSSQL service account
with sysadmin privileges "can be used to log into the SQL Server
instance remotely, read all of the databases... and run operating
system commands through SQL Server."

**MITRE ATT&CK mapping:** T1078.002 (Valid Accounts), T1210
(Exploitation of Remote Services)

**Risk:** Group-membership remediation alone is insufficient for
service accounts with SPNs. The SPN itself encodes a privileged
relationship that BloodHound surfaces independently. This demonstrates
why retesting via fresh full data collection, not just re-checking the
specific thing that was fixed, is essential: it caught a real path a
narrower check would have missed entirely.

**Remediation applied:**
```powershell
setspn -D MSSQLSvc/dc01.corp.local:1433 svc-sql
```
SPN removed since no legitimate SQL Server instance exists on DC01 —
it was registered only to demonstrate Kerberoastability for Finding 2.
Verified via `setspn -L svc-sql`, which returned no registered SPNs
(screenshot 31).

**Retest confirmation:** Confirmed via final combined retest
(screenshot 32, directly comparable to screenshot 25's original
vulnerable state) no SQLAdmin or any other edge from svc-sql to
Domain Admins or DC01 remains.