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