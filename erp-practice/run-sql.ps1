<#
.SYNOPSIS
    一键执行 erp-practice/sql 下的全部脚本（按 01→04 顺序）。

.DESCRIPTION
    用 .NET 的 System.Data.SqlClient 直连本地 SQL Server，不依赖 sqlcmd
    或 ODBC 驱动 —— 这样可以避开"客户端不支持加密"之类的驱动层问题。

    默认连 localhost 的默认实例，Windows 集成认证。

.PARAMETER Server
    服务器地址。命名实例请用 '.\SQLEXPRESS'，LocalDB 用 '(localdb)\MSSQLLocalDB'。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\run-sql.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\run-sql.ps1 -Server '.\SQLEXPRESS'
#>
param(
    [string]$Server = 'localhost'
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlDir    = Join-Path $scriptDir 'sql'

if (-not (Test-Path $sqlDir)) {
    throw "找不到 sql 目录：$sqlDir"
}

$connectionString = "Server=$Server;Database=master;Integrated Security=SSPI;TrustServerCertificate=True;Connect Timeout=10;"

$files = @(
    '01-schema.sql',
    '02-seed.sql',
    '03-report-queries.sql',
    '04-stored-procedures.sql'
)

Write-Host "连接到 $Server ..." -ForegroundColor Cyan

$conn = New-Object System.Data.SqlClient.SqlConnection $connectionString
try {
    $conn.Open()
}
catch {
    Write-Host "连接失败：$($_.Exception.Message)" -ForegroundColor Red
    Write-Host "如果用的是命名实例，加参数：-Server '.\SQLEXPRESS'" -ForegroundColor Yellow
    exit 1
}

$totalBatches = 0
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

foreach ($file in $files) {

    $path = Join-Path $sqlDir $file
    if (-not (Test-Path $path)) {
        Write-Host "  跳过（文件不存在）：$file" -ForegroundColor Yellow
        continue
    }

    # GO 不是 SQL 语句，而是 SSMS / sqlcmd 的批分隔符，
    # 通过代码执行时必须自己按行拆开、逐批提交。
    $text    = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $batches = [regex]::Split($text, '(?im)^\s*GO\s*\r?\n') | Where-Object { $_.Trim().Length -gt 0 }

    $n = 0
    foreach ($batch in $batches) {

        $n++

        $cmd = $conn.CreateCommand()
        $cmd.CommandText    = $batch
        $cmd.CommandTimeout = 300

        try {
            $cmd.ExecuteNonQuery() | Out-Null
        }
        catch {
            Write-Host ''
            Write-Host "[失败] $file 第 $n 批：$($_.Exception.Message)" -ForegroundColor Red
            Write-Host '--- 该批次内容（前 300 字符）---' -ForegroundColor DarkGray
            Write-Host $batch.Substring(0, [Math]::Min(300, $batch.Length))
            $cmd.Dispose()
            $conn.Dispose()
            exit 1
        }
        finally {
            $cmd.Dispose()
        }
    }

    $totalBatches += $n
    Write-Host ("  OK  {0,-26} {1,3} 批" -f $file, $n) -ForegroundColor Green
}

# 汇总一下数据量，顺便证明脚本真的生效了
$summary = $conn.CreateCommand()
$summary.CommandText = @"
SELECT (SELECT COUNT(*) FROM dbo.Material)   AS Materials,
       (SELECT COUNT(*) FROM dbo.BillHead)   AS Bills,
       (SELECT COUNT(*) FROM dbo.BillDetail) AS BillDetails,
       (SELECT COUNT(*) FROM dbo.vw_StockBalance) AS StockRows
"@
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $summary
$result  = New-Object System.Data.DataTable
$adapter.Fill($result) | Out-Null
$row = $result.Rows[0]

$stopwatch.Stop()

Write-Host ''
Write-Host ("完成：{0} 个文件 / {1} 个批次 / {2:N1} 秒" -f $files.Count, $totalBatches, $stopwatch.Elapsed.TotalSeconds) -ForegroundColor Cyan
Write-Host ("数据：{0} 个物料、{1} 张单据、{2} 行明细、库存视图 {3} 行" -f `
    $row.Materials, $row.Bills, $row.BillDetails, $row.StockRows) -ForegroundColor Cyan
Write-Host ''
Write-Host '下一步：用 Visual Studio 打开 csharp\ErpPractice.csproj，F5 运行演示程序。' -ForegroundColor Cyan

$conn.Dispose()
