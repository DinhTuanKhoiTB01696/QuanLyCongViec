Add-Type -AssemblyName System.Drawing

function Generate-Icon ($size, $path) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    
    # Enable anti-aliasing
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text::TextRenderingHint]::AntiAlias
    
    # Draw Background (branding blue #2563EB)
    $color = [System.Drawing.ColorTranslator]::FromHtml("#2563EB")
    $brush = New-Object System.Drawing.SolidBrush($color)
    $g.FillRectangle($brush, 0, 0, $size, $size)
    
    # Draw a stylized white S in the center
    $fontSize = $size * 0.55
    $font = New-Object System.Drawing.Font("Arial", $fontSize, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    
    # Measure text size to center it
    $textSize = $g.MeasureString("S", $font)
    $x = ($size - $textSize.Width) / 2
    $y = ($size - $textSize.Height) / 2 + ($size * 0.05)
    
    $g.DrawString("S", $font, $textBrush, $x, $y)
    
    # Save to PNG
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Clean up
    $brush.Dispose()
    $textBrush.Dispose()
    $font.Dispose()
    $g.Dispose()
    $bmp.Dispose()
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publicDir = Join-Path (Split-Path -Parent $scriptDir) "Frontend\public"

if (!(Test-Path $publicDir)) {
    $publicDir = "c:\Users\phucl\OneDrive\Desktop\QLCV_2\QuanLyCongViec\Frontend\public"
}

Generate-Icon 192 "$publicDir\pwa-192x192.png"
Generate-Icon 512 "$publicDir\pwa-512x512.png"
Write-Host "PWA Icons generated successfully at $publicDir"
