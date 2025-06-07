[CmdletBinding()]
param (
    [Parameter(Mandatory = $false, HelpMessage = "Enter the TCP port you want to use to listen on, for example 3389", parameterSetName = "TCP")]
    [ValidatePattern('^[0-9]+$')]
    [ValidateRange(0, 65535)]
    [int]$Port = 16384,

    [Parameter(Mandatory = $false, HelpMessage = "Enter the path to save the recieved data")]
    [string]$Output,

    [Parameter(Mandatory = $false, HelpMessage = "Run this script in a non-interactive mode")]
    [switch]$NonInteractive,

    [Parameter(Mandatory = $false, HelpMessage = "Connection timeout in seconds (default: 30)")]
    [ValidateRange(1, 3600)]
    [int]$ConnectionTimeoutSeconds = 30,

    [Parameter(Mandatory = $false, HelpMessage = "Data timeout in seconds, resets when data arrives (default: 30)")]
    [ValidateRange(1, 3600)]
    [int]$DataTimeoutSeconds = 30
)

$ErrorActionPreference = 'Stop'

$Global:ProgressPreference = 'SilentlyContinue' # Hide GUI output

function Wait-TcpConnection ($listener, $timeoutSeconds) {
    if ($NonInteractive) {
        Write-Host ("Waiting for an incoming connection (timeout: {0}s)..." -f $timeoutSeconds) -ForegroundColor Green
    } else {
        Write-Host ("Waiting for an incoming connection (timeout: {0}s), press Escape to stop listening..." -f $timeoutSeconds) -ForegroundColor Green
    }
    
    $startTime = Get-Date
    while (!$listener.Pending()) {
        # Check if we've exceeded the timeout
        $elapsed = (Get-Date) - $startTime
        if ($elapsed.TotalSeconds -ge $timeoutSeconds) {
            Write-Host ("Connection timeout after {0} seconds" -f $timeoutSeconds) -ForegroundColor Yellow
            return $false
        }
        
        # Only check for key press in interactive mode
        if (!$NonInteractive -and $host.UI.RawUI.KeyAvailable) {
            $key = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp,IncludeKeyDown")
            if ($key.VirtualKeyCode -eq 27) {
                return $false
            }
        }
        Start-Sleep -Milliseconds 1000
    }
    return $true
}

try {
    $testClient = New-Object System.Net.Sockets.TcpClient
    $testClient.Connect("localhost", $Port)
    $testClient.Close()
    Write-Warning ("TCP Port {0} is already listening, aborting." -f $Port)
    return
} catch {
    Write-Host ("TCP port {0} is available, continuing..." -f $Port) -ForegroundColor Green
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
    if (!(Wait-TcpConnection $listener $ConnectionTimeoutSeconds)) {
        break
    }

    Write-Host ("Incomming connection, responding...") -ForegroundColor Green
    $client = $listener.AcceptTcpClient()

    Write-Host ("Connection established, reading data (timeout: {0}s)..." -f $DataTimeoutSeconds) -ForegroundColor Green
    $text = ''
    $stream = $client.GetStream()
    $bytes = New-Object System.Byte[] 1024
    $dataStartTime = Get-Date
    
    # Set read timeout on the stream (in milliseconds)
    $stream.ReadTimeout = $DataTimeoutSeconds * 1000
    
    try {
        while (($i = $stream.Read($bytes, 0, $bytes.Length)) -ne 0) {
            # Reset timeout when data arrives
            $dataStartTime = Get-Date
            
            $EncodedText = New-Object System.Text.ASCIIEncoding
            $data = $EncodedText.GetString($bytes, 0, $i)
            $text += $data
            Write-Output $data
        }
    } catch [System.IO.IOException] {
        # Check if this was a timeout or connection closed
        $elapsed = (Get-Date) - $dataStartTime
        if ($elapsed.TotalSeconds -ge ($DataTimeoutSeconds - 1)) {
            Write-Host ("Data timeout after {0} seconds" -f [math]::Round($elapsed.TotalSeconds)) -ForegroundColor Yellow
        }
        # If it's not a timeout, it's likely the connection was closed normally
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
