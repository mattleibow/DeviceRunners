[CmdletBinding()]
param (
    [Parameter(Mandatory = $false, HelpMessage = "Enter the TCP port you want to use to listen on, for example 3389", parameterSetName = "TCP")]
    [ValidatePattern('^[0-9]+$')]
    [ValidateRange(0, 65535)]
    [int]$Port = 16384,

    [Parameter(Mandatory = $false, HelpMessage = "Enter the path to save the recieved data")]
    [string]$Output,

    [Parameter(Mandatory = $false, HelpMessage = "Run this script in a non-interactive mode")]
    [switch]$NonInteractive
)

$ErrorActionPreference = 'Stop'

$Global:ProgressPreference = 'SilentlyContinue' # Hide GUI output

function Wait-TcpConnection ($listener) {
    Write-Host ("Waiting for an incoming connection...") -ForegroundColor Green
    while (!$listener.Pending()) {
        if ($host.UI.RawUI.KeyAvailable) {
            $key = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp,IncludeKeyDown")
            if ($key.VirtualKeyCode -eq 27) {
                return $false
            }
        }
        Start-Sleep -Milliseconds 1000
    }
    return $true
}

$testPort = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction Stop
if ($testPort.TcpTestSucceeded -ne $True) {
    Write-Host ("TCP port {0} is available, continuing..." -f $Port) -ForegroundColor Green
} else {
    Write-Warning ("TCP Port {0} is already listening, aborting." -f $Port)
    return
}

# Start TCP Server
$ipendpoint = New-Object System.Net.IPEndPoint([ipaddress]::Any, $Port)
$listener = New-Object System.Net.Sockets.TcpListener $ipendpoint
$listener.Start()

Write-Host ("Now listening on TCP port {0}, press Escape to stop listening." -f $Port) -ForegroundColor Green
if ($NonInteractive) {
    Write-Host "Listening in non-interactive mode, will terminate after first connection." -ForegroundColor Green
}

while ($true) {
    if (!(Wait-TcpConnection ($listener))) {
        break
    }

    Write-Host ("Incomming connection, responding...") -ForegroundColor Green
    $client = $listener.AcceptTcpClient()

    Write-Host ("Connection established, reading data...") -ForegroundColor Green
    $text = ''
    $stream = $client.GetStream()
    $bytes = New-Object System.Byte[] 1024
    while (($i = $stream.Read($bytes, 0, $bytes.Length)) -ne 0){
        $EncodedText = New-Object System.Text.ASCIIEncoding
        $data = $EncodedText.GetString($bytes, 0, $i)
        $text += $data
        Write-Output $data
    }
    $stream.close()
    $client.Close()

    # Recieved a "ping" so we can discard and wait again
    if ($text.Trim() -ne 'ping') {
        if ($Output) {
            $text | Set-Content $Output
        }

        if ($NonInteractive) {
            break
        }
    }
}

$listener.Stop()
Write-Host ("Stopped listening on TCP port {0}" -f $Port) -ForegroundColor Green
