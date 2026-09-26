New-ADUser `
	-Name "svc-sql" `
	-SamAccountName "svc-sql" `
	-UserPrincipalName "svc-sql@corp.local" `
	-Path "OU=IT,DC=corp,DC=local" `
	-AccountPassword (ConvertTo-SecureString "December2024!" -AsPlainText -Force) `
	-Enabled $true `
	-PasswordNeverExpires $true `
	-ChangePasswordAtLogon $false
	
setspn -A MSSQLSvc/dc01.corp.local:1433 svc-sql

Add-ADGroupMember -Identity "Domain Admins" -Members "svc-sql"