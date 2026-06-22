$apiKey = "re_cpa1PYgR_FxurYBvgnbZYcadyhkyo2TCL"
$body = @{
    from = "noreply@sprinta.io.vn"
    to = @("phatbttb01771@gmail.com")
    subject = "Test OTP Email"
    html = "<p>Test: Ma OTP cua ban la <b>123456</b></p>"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest `
        -Uri "https://api.resend.com/emails" `
        -Method Post `
        -Headers @{ "Authorization" = "Bearer $apiKey"; "Content-Type" = "application/json" } `
        -Body $body `
        -UseBasicParsing
    Write-Output "STATUS: $($response.StatusCode)"
    Write-Output "BODY: $($response.Content)"
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errorBody = $reader.ReadToEnd()
    Write-Output "ERROR STATUS: $statusCode"
    Write-Output "ERROR BODY: $errorBody"
}
