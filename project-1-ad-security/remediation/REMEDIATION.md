# Remediation Log

All four findings identified during this assessment were remediated
and retested. Full technical detail for each finding, including
before/after evidence, is in [FINDINGS.md](../docs/FINDINGS.md) this
file is a condensed action log.

## Finding 1: Nested Group Privilege Escalation

**Date remediated:** September 26, 2026

**Action:**
```powershell
Remove-ADGroupMember -Identity "IT Admins" -Members "Help Desk Support" -Confirm:$false
```

**Before:** bocasio and aalvarez held unintended Domain Admin rights
via a nested group chain (Help Desk Support → IT Admins → Domain
Admins).

**After:** Nesting removed. Verified via recursive group membership
check (screenshot 26) and confirmed via BloodHound retest (screenshot
27).

**Recommended long-term fix:** Replace nested group membership for
help desk staff with scoped, delegated permissions rather than
blanket membership in privileged groups. Conduct periodic recursive
group membership audits, this class of finding is invisible to a
flat membership review.

---

## Finding 2: Kerberoastable Service Account with Direct Domain Admin Membership

**Date remediated:** September 26, 2026

**Actions:**
```powershell
Remove-ADGroupMember -Identity "Domain Admins" -Members "svc-sql" -Confirm:$false
Set-ADAccountPassword -Identity "svc-sql" -Reset -NewPassword (secure string)
```

**Before:** svc-sql held direct Domain Admin membership with a weak,
non-expiring password.

**After:** Group membership removed; password rotated to a strong
value. Verified via recursive group membership check (screenshot 28).

**Recommended long-term fix:** Migrate to a Group Managed Service
Account (gMSA) so Windows manages the credential automatically with no
human-known password, eliminating this class of risk entirely.

**Note:** This remediation alone was incomplete (see Finding 4)

---

## Finding 3: GenericAll Rights via Account Operators

**Date remediated:** September 27, 2026

**Confirmed before removal:** Account Operators group membership was
empty, this finding reflects a latent default-permission risk, not
active exploitation.

**Action:**
```powershell
$acl.RemoveAccessRule($aceToRemove)
Set-Acl "AD:\$($ws01.DistinguishedName)" -AclObject $acl
```

**Before:** BUILTIN\Account Operators held GenericAll rights over
WS01's computer object.

**After:** ACE removed. Verified via `Get-Acl` returning no Account
Operators entry (screenshot 30).

**Recommended long-term fix:** Audit Account Operators group membership
regularly; this built-in group is often unused in modern environments
and should be emptied or have its default rights restricted where
organizational policy allows.

---

## Finding 4: SQLAdmin Relationship (SPN-Based, Independent of Group Membership)

**Date remediated:** September 27, 2026

**Action:**
```powershell
setspn -D MSSQLSvc/dc01.corp.local:1433 svc-sql
```

**Before:** svc-sql's SPN caused BloodHound to infer SQL sysadmin
rights on DC01 itself, independent of the (already-removed) Domain
Admin group membership.

**After:** SPN removed. Verified via `setspn -L svc-sql` returning no
registered SPNs (screenshot 31).

**Recommended long-term fix:** Never run SQL Server directly on a
domain controller, a well-established AD hardening principle. Where
unavoidable, the SQL service account must be scoped to least
privilege, isolated from domain-controller-level trust entirely.

---

## Final Verification

**Date:** September 27, 2026

Full combined retest performed after all four remediations: BloodHound
data cleared, fresh SharpHound collection run, re-imported, and the
same pathfinding query re-executed. **Zero paths from any low-privilege
account to Domain Admins remain.** See screenshot 32, directly
comparable to screenshot 25 (original vulnerable state).