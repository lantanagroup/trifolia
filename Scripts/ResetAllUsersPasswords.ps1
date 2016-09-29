Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string] $Auth0Account,

    [Parameter(Mandatory=$True, Position=2)]
    [string] $Auth0Connection,
    
    [Parameter(Mandatory=$True, Position=3)]
    [string] $Auth0ClientId
)

$users = Get-TrifoliaUsers
$users = $users.GetList()

for ($i = 0; $i -lt $users.Count; $i++)
{

    $user = $users[$i]

    if ($i % 20 -eq 0) {
        sleep 2
    }

    Set-TrifoliaPasswordReset -Auth0Account $Auth0Account -Auth0Connection $Auth0Connection -Auth0ClientId $Auth0ClientId -Emails $user.Email

    $count = $count + 1
}