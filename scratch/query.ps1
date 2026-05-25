$connString = "Server=.;Database=UretimPlanlama;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT t.name AS TableName, c.name AS ColumnName FROM sys.tables t JOIN sys.columns c ON t.object_id = c.object_id JOIN sys.types y ON c.user_type_id = y.user_type_id WHERE y.name IN ('varchar', 'nvarchar', 'char', 'nchar')"
$columns = @()
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    $columns += [PSCustomObject]@{
        Table = $reader["TableName"]
        Column = $reader["ColumnName"]
    }
}
$conn.Close()

foreach ($col in $columns) {
    $conn.Open()
    $chkCmd = $conn.CreateCommand()
    $tableName = $col.Table
    $colName = $col.Column
    $chkCmd.CommandText = "SELECT COUNT(*) FROM [$tableName] WHERE [$colName] LIKE '%ATILDI%'"
    $count = $chkCmd.ExecuteScalar()
    if ($count -gt 0) {
        Write-Output "Found in Table: $tableName, Column: $colName ($count rows)"
    }
    $conn.Close()
}
