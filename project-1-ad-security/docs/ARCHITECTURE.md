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