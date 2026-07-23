$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = 'Server=KHOI\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True'
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'ALTER DATABASE TaskManagementDB SET MULTI_USER WITH ROLLBACK IMMEDIATE'
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host 'Done'
