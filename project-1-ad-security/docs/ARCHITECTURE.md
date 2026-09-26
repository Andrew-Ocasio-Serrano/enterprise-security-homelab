## Environment Build Details
- OS: Windows Server 2022 Standard (Desktop Experience)
- Source: Microsoft Evaluation Center
- Hypervisor: VMware Workstation Player
- DC01: 4GB RAM, 2 vCPU, 60GB disk, NAT networking

## Build Note
Initial install attempt failed with "Windows cannot find the Microsoft
Software License Terms" due to a stray autoinstall floppy device VMware
attached during VM creation (tied to the Easy Install wizard). Resolved
by removing the floppy device from VM settings and reinstalling — this
allowed Windows Setup to run interactively instead of via a broken
unattended-install configuration.

Note: Forest/domain functional level defaulted to Windows Server 2016
during promotion (the wizard's default when accepting standard options
on 2022 media). Left as-is since functional level doesn't affect this
project's AD security assessment goals.

## Attacker/Collector VM
- Kali Linux (official VMware image from kali.org)
- 4GB RAM, 2 vCPU, NAT networking (same virtual network as DC01)
- Used for BloodHound CE (Docker) and SharpHound collection

## Network Verification
Confirmed connectivity between Kali (attacker/collector) and DC01
(domain controller) via ping across the shared NAT network — required
for later SharpHound collection.

Note: Screenshot 17 substituted docker ps output for the original
docker compose up -d startup log, which had scrolled out of terminal
history by the time of capture — docker ps provides equivalent proof
of running container state.

## Workstation VM — Note on Initial Login
The Windows setup wizard created a local account also named "corp.local"
during the domain-account configuration step, easy to confuse with an
actual domain login. First whoami check (desktop-1ojk4d2\corp.local)
confirmed this was a local account session, not a domain one. Corrected
by logging in via "Other user" with a real domain account (CORP\bocasio)
instead — verified via whoami returning corp\bocasio (screenshot 22).