# Architecture

## Environment Build Details

- **DC01:** Windows Server 2022 Standard (Desktop Experience),
  Microsoft Evaluation Center ISO, VMware Workstation Player, 4GB RAM,
  2 vCPU, 60GB disk, NAT networking, static IP 192.168.243.10
- **WS01:** Windows 11 Enterprise Evaluation (90-day), 2-3GB RAM, 2 vCPU,
  NAT networking, static IP 192.168.243.20, DNS pointed to DC01
- **Kali:** Official Kali Linux VMware image, 4GB+ RAM, NAT networking
- **Domain:** corp.local (NetBIOS: CORP), forest/domain functional
  level Windows2016Forest (wizard default, not manually raised, no
  impact on this project's assessment goals)

## Note: Windows 10 → Windows 11 Substitution

WS01 was originally planned as Windows 10 Enterprise Evaluation, but
Windows 10 reached end-of-life in October 2025 and is no longer
available via Microsoft's Evaluation Center. Substituted Windows 11
Enterprise Evaluation, which requires a virtual TPM (auto-configured
by VMware) no functional impact on domain-join or SharpHound
collection goals.

## Note: Initial Installer Failure (Floppy Autoinstall)

The first DC01 install attempt failed with "Windows cannot find the
Microsoft Software License Terms," caused by a stray autoinstall
floppy device VMware attached during VM creation (tied to the "Easy
Install" wizard). Resolved by removing the floppy device from VM
settings before reinstalling, allowing Windows Setup to run
interactively.

## Note: DNS Self-Reference Before AD DS Installation

Setting DC01's own DNS server to 127.0.0.1 before the AD DS/DNS role
was installed caused a temporary loss of internet access on the VM
(expected) since no DNS service was running yet to resolve that
loopback. Resolved once AD DS/DNS installation completed.

## Note: DNS Forwarder Configuration

DC01's DNS server only resolves the corp.local zone by default. Added
public DNS forwarders (8.8.8.8, 1.1.1.1) to restore internet name
resolution for the domain-joined environment, standard real-world DC
configuration, not just a lab workaround.

## Note: Local Account vs. Domain Account Confusion on WS01

The Windows setup wizard created a local account also named
"corp.local" during domain configuration, easy to confuse with an
actual domain login. First `whoami` check on WS01
(`desktop-1ojk4d2\corp.local`) confirmed this was a local session, not
a domain one. The computer's hostname had also not been renamed to
WS01 as intended. Corrected by logging in via "Other user" with a real
domain account (CORP\bocasio), confirmed via `whoami` returning
`corp\bocasio`.

## Attacker/Collector VM (Kali)

Used for BloodHound Community Edition (via Docker) and SharpHound
collection. Network connectivity to DC01 and WS01 confirmed via ping
across the shared NAT network prior to tool installation.

## Note: BloodHound Docker Port Binding

BloodHound CE's default `docker-compose.yml` binds port 8080 to
127.0.0.1 (Kali's loopback only), blocking access from other VMs —
confirmed via `Test-NetConnection` from WS01 (ping succeeded, TCP
connection failed). Fixed by changing the port binding to `0.0.0.0` in
`docker-compose.yml`, exposing it to the shared NAT network.

## Note: SharpHound Flagged by Windows Defender SmartScreen

SharpHound's download was blocked by SmartScreen on WS01, correctly
identifying it as a credential/AD-enumeration tool matching known
attacker tooling signatures (expected) standard AV/EDR behavior.
Resolved via the browser's explicit "Keep" override rather than
disabling Defender's real-time protection wholesale (which was tried
first and caused a separate download failure due to Defender's network
filtering component also being affected).

## Collection Methodology

Data collected via SharpHound v2.16.0, run as a standard, non-
privileged domain user (bocasio) from a domain-joined workstation
(WS01) not from the domain controller, to accurately simulate what
an attacker with only basic user-level access could enumerate.
Collection completed in under 2 seconds, covering 315 AD objects, with
zero access-denied errors: a completely unprivileged user was able to
enumerate the full domain structure using only default AD read
permissions. This baseline visibility is itself worth noting: default
AD configurations grant broad structural visibility to any
authenticated user, which is precisely what makes attack-path
discovery possible for real attackers with minimal initial access.

## Note: BloodHound Data Accumulation Across Uploads

BloodHound CE merges new SharpHound uploads into existing graph data
rather than replacing it, meaning a remediated relationship can persist
visually in the graph from a prior collection until the database is
cleared and re-imported fresh. For accurate before/after retesting,
BloodHound's data was cleared via the Administration page before every
post-remediation upload.

## Lessons Learned

**Naming and configuration details matter more than they seem.** A
stray autoinstall floppy, a DNS self-reference set before the DNS role
existed, and a local account sharing a name with the domain all caused
real friction, each traced back to a small, specific configuration
detail rather than a fundamental misunderstanding.

**PowerShell scripting errors are usually about syntax precision, not
logic.** String interpolation requiring `$()`, exact `-Path` formatting,
and full parameter names (`-Enabled`, not the abbreviated `-Enable`)
were all real bugs caught before running against the live domain, 
verifying command syntax carefully before execution matters as much
as understanding what the command is supposed to do.

**Remediating one path doesn't guarantee every path is closed.**
Removing svc-sql's Domain Admin group membership (Finding 2) left a
completely separate `SQLAdmin` relationship intact, rooted in the
account's SPN rather than its group membership, invisible until a
fresh BloodHound retest surfaced it as Finding 4. This is the single
most important technical lesson from this project: security tooling
that automates graph analysis catches relationships that a manual
"did we fix the thing we found" check would miss entirely.

**Security tools are, correctly, treated as suspicious by other
security tools.** SharpHound being flagged by Windows Defender
SmartScreen isn't a bug to work around carelessly, it's expected,
correct behavior, and real-world use of tools like this in an
authorized assessment requires deliberate allowlisting, which is
itself a relevant operational consideration.

**Verification commands are only useful if you actually look at their
output.** Several fixes in this project (group membership removal, ACL
changes, SPN removal) required a dedicated verification step
immediately after, not because the primary command was likely to
silently fail, but because the discipline of checking prevented
several near-misses (like almost documenting a screenshot before
confirming what it actually showed).
