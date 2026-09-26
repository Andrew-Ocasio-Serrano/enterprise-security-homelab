$users = @(
	@{First="Bob";	Last="Ocasio";	OU="IT"},
	@{First="Andres";	Last="Alvarez";	OU="IT"},
	@{First="Lin";	Last="Shu";	OU="IT"},
	@{First="Robert";	Last="Toledo";	OU="Finance"},
	@{First="Creig";	Last="Smith";	OU="Finance"},
	@{First="Mateo";	Last="Torres";	OU="Finance"},
	@{First="Michelle";	Last="Brad";	OU="HR"},
	@{First="Jessy";	Last="Dunn";	OU="HR"},
	@{First="Steven";	Last="Wilson";	OU="Executives"},
	@{First="Kevin";	Last="Lopez";	OU="Executives"}
)

foreach ($u in $users) {
	$sam = ($u.First.Substring(0,1) + $u.Last).ToLower()
	New-ADUser `
		-Name "$($u.First) $($u.Last)" `
		-GivenName $u.First `
		-Surname $u.Last `
		-SamAccountName $sam `
		-UserPrincipalName "$sam@corp.local" `
		-Path "OU=$($u.OU),DC=corp,DC=local" `
		-AccountPassword (ConvertTo-SecureString "temporaryPass123!" -AsPlainText -Force) `
		-Enabled $true `
		-ChangePasswordAtLogon $true
}