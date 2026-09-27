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

## Network Note — BloodHound Docker Port Binding
BloodHound CE's default docker-compose.yml binds port 8080 to
127.0.0.1 (Kali's loopback only), which blocks access from other VMs
on the network — confirmed via Test-NetConnection from WS01 (ping
succeeded, TCP connection failed). Fixed by changing the port binding
to 0.0.0.0 in docker-compose.yml, exposing it to the shared NAT
network so WS01 can reach BloodHound's web UI directly.

## Note: SharpHound Flagged by Windows Defender SmartScreen
SharpHound's download was blocked by SmartScreen on WS01, correctly
identifying it as a credential/AD-enumeration tool matching known
attacker tooling signatures. This is expected, standard AV/EDR
behavior — real-world SharpHound usage in an authorized assessment
requires similar allowlisting or execution from memory to avoid
detection, which is itself a relevant operational security
consideration for red-team engagements.

## Note: SharpHound Download Friction on WS01
Multiple layers of Windows Defender protection interfered with
downloading SharpHound: SmartScreen initially blocked it as unsafe;
disabling real-time protection to work around this caused a separate
download failure, likely due to Defender's network filtering component
also being affected. Resolved by re-enabling protection and downloading
via PowerShell's Invoke-WebRequest directly, bypassing browser-level
SmartScreen interception.