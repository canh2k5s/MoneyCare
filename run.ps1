
$projectUrl = "http://localhost:5158/"

# Chạy ASP.NET Core
$process = Start-Process dotnet `
    -ArgumentList "run --launch-profile http" `
    -PassThru `
    -NoNewWindow

# Chờ website khởi động
Write-Host "Dang cho MoneyCare khoi dong..."

$started = $false

for ($i = 0; $i -lt 30; $i++) {
    try {
        $response = Invoke-WebRequest $projectUrl -TimeoutSec 2
        $started = $true
        break
    }
    catch {
        Start-Sleep -Seconds 1
    }
}

if ($started) {
    Start-Process "C:\Program Files\CocCoc\Browser\Application\browser.exe" $projectUrl
    Write-Host "MoneyCare da mo tren Coc Coc!"
}
else {
    Write-Host "Khong the khoi dong website. Hay kiem tra loi Terminal."
}
