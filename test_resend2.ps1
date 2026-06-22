$apiKey = "re_cpa1PYgR_FxurYBvgnbZYcadyhkyo2TCL"

$bodyObj = [ordered]@{
    from    = "noreply@sprinta.io.vn"
    to      = @("phatbttb01771@gmail.com")
    subject = "Ma xac thuc OTP - SprintA"
    html    = "<h2>Ma OTP cua ban la: <b>888999</b></h2><p>Ma co hieu luc 5 phut.</p>"
}
$bodyJson = $bodyObj | ConvertTo-Json -Depth 5

Write-Output "=== SENDING TO: phatbttb01771@gmail.com ==="
Write-Output "=== BODY JSON: $bodyJson ==="

try {
    $result = Invoke-WebRequest `
        -Uri "https://api.resend.com/emails" `
        -Method POST `
        -Headers @{ Authorization = "Bearer $apiKey" } `
        -ContentType "application/json" `
        -Body $bodyJson `
        -UseBasicParsing
    Write-Output "=== HTTP STATUS: $($result.StatusCode) ==="
    Write-Output "=== RESPONSE: $($result.Content) ==="
} catch {
    Write-Output "=== ERROR STATUS: $($_.Exception.Response.StatusCode.value__) ==="
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = [System.IO.StreamReader]::new($stream)
    Write-Output "=== ERROR BODY: $($reader.ReadToEnd()) ==="
}
