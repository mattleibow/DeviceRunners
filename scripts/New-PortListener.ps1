function New-PortListener {
    [CmdletBinding(DefaultParameterSetName = 'All')]
    param (
        [parameter(Mandatory = $false, HelpMessage = "Enter the tcp port you want to use to listen on, for example 3389", parameterSetName = "TCP")]
        [ValidatePattern('^[0-9]+$')]
        [ValidateRange(0, 65535)]
        [int]$Port,
        
        [string]$Output
    )

    $Global:ProgressPreference = 'SilentlyContinue' # Hide GUI output

    $testPort = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction Stop
    if ($testPort.TcpTestSucceeded -ne $True) {
        Write-Host ("TCP port {0} is available, continuing..." -f $Port) -ForegroundColor Green
    }
    else {
        Write-Warning ("TCP Port {0} is already listening, aborting..." -f $Port)
        return
    }

    # Start TCP Server
    # Used procedure from https://riptutorial.com/powershell/example/18117/tcp-listener
    $ipendpoint = New-Object System.Net.IPEndPoint([ipaddress]::Any, $Port)
    $listener = New-Object System.Net.Sockets.TcpListener $ipendpoint
    $listener.Start()

    Write-Host ("Now listening on TCP port {0}, press Escape to stop listening" -f $Port) -ForegroundColor Green
    while ($true) {
        Write-Host ("Waiting for an incoming connection...") -ForegroundColor Green
        while (!$listener.Pending()) {
            if ($host.UI.RawUI.KeyAvailable) {
                $key = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp,IncludeKeyDown")
                if ($key.VirtualKeyCode -eq 27) {
                    $listener.Stop()
                    Write-Host ("Stopped listening on TCP port {0}" -f $Port) -ForegroundColor Green
                    return
                }
            }
            Start-Sleep -Milliseconds 1000
        }
        $client = $listener.AcceptTcpClient()

        Write-Host ("Connection established, reading data...") -ForegroundColor Green
        $text = ""
        $stream = $client.GetStream()
        $bytes = New-Object System.Byte[] 1024
        while (($i = $stream.Read($bytes,0,$bytes.Length)) -ne 0){
            $EncodedText = New-Object System.Text.ASCIIEncoding
            $data = $EncodedText.GetString($bytes,0, $i)
            $text += $data
            Write-Output $data
        }
        $stream.close()
        $client.Close()

        if ($Output) {
            $text | Set-Content $Output
        }
    }
}
