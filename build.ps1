param([Parameter(Mandatory=$true)][string]$KeePassPath)
$ErrorActionPreference = 'Stop'
$exe = (Resolve-Path -LiteralPath (Join-Path $KeePassPath 'KeePass.exe')).Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (!(Test-Path -LiteralPath $compiler)) { throw 'Install .NET Framework 4.x with the C# compiler first.' }
$output = Join-Path $PSScriptRoot 'dist\CustomTheme'
New-Item -ItemType Directory -Force -Path $output | Out-Null
& $compiler /nologo /target:library /optimize+ /reference:$exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll /out:"$output\CustomTheme.dll" "$PSScriptRoot\src\CustomTheme.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'palettes') -Destination $output -Recurse -Force
Get-FileHash -Algorithm SHA256 -LiteralPath "$output\CustomTheme.dll"
