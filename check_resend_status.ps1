$apiKey = "re_cpa1PYgR_FxurYBvgnbZYcadyhkyo2TCL"
$emailId = "bce05996-8dba-4104-a4a8-0a7621923b67"

Write-Output "=== CHECKING EMAIL STATUS: $emailId ==="

try {
    $result = Invoke-WebRequest `
        -Uri "https://api.resend.com/emails/$emailId" `
        -Method GET `
        -Headers @{ Authorization = "Bearer $apiKey" } `
        -UseBasicParsing
    Write-Output "=== HTTP STATUS: $($result.StatusCode) ==="
    Write-Output "=== RESPONSE: $($result.Content) ==="
} catch {
    Write-Output "=== ERROR STATUS: $($_.Exception.Response.StatusCode.value__) ==="
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = [System.IO.StreamReader]::new($stream)
    Write-Output "=== ERROR: $($reader.ReadToEnd()) ==="
}
