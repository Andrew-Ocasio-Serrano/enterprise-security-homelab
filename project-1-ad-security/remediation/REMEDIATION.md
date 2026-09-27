## Finding 1: Nested Group Privilege Escalation — REMEDIATED

**Date remediated: September 26, 2026** 

**Action taken:**
```powershell
Remove-ADGroupMember -Identity "IT Admins" -Members "Help Desk Support" -Confirm:$false
```

**Before:** Help Desk Support nested inside IT Admins, which was nested
inside Domain Admins — granting bocasio and aalvarez unintended Domain
Admin rights.

**After:** Help Desk Support removed from IT Admins. Verified via
`Get-ADGroupMember -Identity "Domain Admins" -Recursive`, which now
returns only the built-in Administrator account.

**Retest confirmation:** Confirmed via Get-ADGroupMember -Recursive on
DC01, bocasio and aalvarez no longer appear in Domain Admins
membership. svc-sql remains, as expected (separate, unremediated
finding). BloodHound-level retest pending (Step 32).

**Recommended long-term fix:** Replace nested group membership for
help desk staff with scoped, delegated permissions (e.g., password
reset rights via ACLs on specific OUs) rather than blanket group
membership in privileged groups. Conduct periodic recursive group
membership audits — this class of finding is invisible to a flat
membership review.

**Retest confirmation:** Confirmed via fresh SharpHound collection and
BloodHound re-import (database cleared before re-upload to ensure
accurate current-state data). The AALVAREZ/BOCASIO → Help Desk Support
→ IT Admins → Domain Admins path no longer appears in the graph. See
screenshot 27, directly comparable to screenshot 25 (pre-remediation).