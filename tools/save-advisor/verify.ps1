param(
  [Parameter(Mandatory=$true)][string]$Snapshot,
  [Parameter(Mandatory=$true)][string]$LiveFlagg,
  [Parameter(Mandatory=$true)][string]$RemoteRoot,
  [Parameter(Mandatory=$true)][string]$Brief,
  [Parameter(Mandatory=$true)][string]$Output,
  [string]$RemoteHost = 'roland'
)
$ErrorActionPreference = 'Stop'
$Output = [IO.Path]::GetFullPath($Output)
foreach ($inputRoot in @($Snapshot,$LiveFlagg)) {
  $relative = [IO.Path]::GetRelativePath([IO.Path]::GetFullPath($inputRoot),$Output)
  if ($relative -eq '.' -or ($relative -ne '..' -and !$relative.StartsWith('..'+[IO.Path]::DirectorySeparatorChar) -and ![IO.Path]::IsPathRooted($relative))) { throw 'Output must be outside input trees' }
}
New-Item -ItemType Directory -Force -Path $Output | Out-Null
$verdictPath=Join-Path $Output 'verdict.json'
# Write an explicit incomplete verdict before any operation that can fail.
[pscustomobject]@{ok=$false; phase='started'; reason='verification did not complete'} | ConvertTo-Json | Set-Content -Encoding utf8 $verdictPath
$here = $PSScriptRoot
function LocalManifest($root) {
  $resolved = (Resolve-Path -LiteralPath $root).Path.TrimEnd('\')
  @(Get-ChildItem -LiteralPath $resolved -Recurse -File | ForEach-Object {
    [pscustomobject]@{ path=$_.FullName.Substring($resolved.Length+1).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
  } | Sort-Object path)
}
function RemoteManifest {
  $escaped = $RemoteRoot.Replace("'", "''")
  $script = @"
`$ErrorActionPreference='Stop'; `$ProgressPreference='SilentlyContinue'; `$root='$escaped'; `$items=@(Get-ChildItem -LiteralPath `$root -Recurse -File | ForEach-Object { [pscustomobject]@{path=`$_.FullName.Substring(`$root.Length+1).Replace('\','/'); sha256=(Get-FileHash -Algorithm SHA256 -LiteralPath `$_.FullName).Hash.ToLowerInvariant()} } | Sort-Object path); ConvertTo-Json -InputObject `$items -Compress
"@
  $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($script))
  $json=ssh -o BatchMode=yes -o ConnectTimeout=8 $RemoteHost powershell -NoProfile -EncodedCommand $encoded
  if ($LASTEXITCODE -ne 0) { throw 'remote manifest failed' }
  $data=@($json | ConvertFrom-Json)
  if ($data.Count -lt 1 -or !($data[0].path) -or !($data[0].sha256)) { throw 'remote manifest incomplete' }
  return $data
}
function SaveManifest($data,$name) { ConvertTo-Json -InputObject @($data) -Depth 4 | Set-Content -Encoding utf8 (Join-Path $Output $name) }
function SameManifest($a,$b) {
  $x=@($a | ForEach-Object { $_.path + ' ' + $_.sha256 })
  $y=@($b | ForEach-Object { $_.path + ' ' + $_.sha256 })
  return ($x.Count -eq $y.Count -and @(Compare-Object $x $y).Count -eq 0)
}
$localBefore=LocalManifest $LiveFlagg
$remoteBefore=RemoteManifest
$snapshotFlaggBefore=LocalManifest (Join-Path $Snapshot 'flagg')
$snapshotRolandBefore=LocalManifest (Join-Path $Snapshot 'roland')
SaveManifest $localBefore 'flagg-before.json'
SaveManifest $remoteBefore 'roland-before.json'
SaveManifest $snapshotFlaggBefore 'snapshot-flagg.json'
SaveManifest $snapshotRolandBefore 'snapshot-roland.json'
$snapshotHashes=[ordered]@{flagg=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $Output 'snapshot-flagg.json')).Hash.ToLowerInvariant(); roland=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $Output 'snapshot-roland.json')).Hash.ToLowerInvariant()}
$briefHash=(Get-FileHash -Algorithm SHA256 -LiteralPath $Brief).Hash.ToLowerInvariant()
$snapshotMatches=SameManifest @($localBefore | Where-Object { $_.path -like 'SaveGame_4/*' }) @($snapshotFlaggBefore | Where-Object { $_.path -like 'SaveGame_4/*' })
$steps=[ordered]@{}
try {
  $steps.cleanBefore = if (@(& git -C $here status --porcelain --untracked-files=normal).Count -eq 0) { 0 } else { 1 }
  & node --test (Join-Path $here 'advisor.test.ts') *> (Join-Path $Output 'tests.txt'); $steps.tests=$LASTEXITCODE
  & node (Join-Path $here 'advisor.ts') (Join-Path $Snapshot 'flagg/SaveGame_4') (Join-Path $Output 'slot4') *> (Join-Path $Output 'slot4.txt'); $steps.slot4=$LASTEXITCODE
  & node (Join-Path $here 'advisor.ts') (Join-Path $Snapshot 'flagg/SaveGame_1') (Join-Path $Output 'slot1') *> (Join-Path $Output 'slot1.txt'); $steps.slot1=$LASTEXITCODE
  & python -I -X utf8 (Join-Path $here 'corpus.py') $Snapshot $Output *> (Join-Path $Output 'corpus.txt'); $steps.corpus=$LASTEXITCODE
  & python -I -X utf8 (Join-Path $here 'crosscheck.py') (Join-Path $Snapshot 'flagg/SaveGame_4') (Join-Path $Output 'slot4/digest.json') (Join-Path $Output 'crosscheck.json') *> (Join-Path $Output 'crosscheck.txt'); $steps.crosscheck=$LASTEXITCODE
  & python -I -X utf8 (Join-Path $here 'negative_checks.py') (Join-Path $Snapshot 'flagg/SaveGame_4') $Brief $Output *> (Join-Path $Output 'negative.txt'); $steps.negative=$LASTEXITCODE
  & node (Join-Path $here 'lint-brief.ts') $Brief (Join-Path $Output 'slot4/digest.json') *> (Join-Path $Output 'brief-lint.txt'); $steps.brief=$LASTEXITCODE
  $steps.cleanAfter = if (@(& git -C $here status --porcelain --untracked-files=normal).Count -eq 0) { 0 } else { 1 }
} finally {
  $localAfter=LocalManifest $LiveFlagg
  $remoteAfter=RemoteManifest
  $snapshotFlaggAfter=LocalManifest (Join-Path $Snapshot 'flagg')
  $snapshotRolandAfter=LocalManifest (Join-Path $Snapshot 'roland')
  SaveManifest $localAfter 'flagg-after.json'
  SaveManifest $remoteAfter 'roland-after.json'
  $localSame=SameManifest $localBefore $localAfter
  $remoteSame=SameManifest $remoteBefore $remoteAfter
  $snapshotSame=(SameManifest $snapshotFlaggBefore $snapshotFlaggAfter) -and (SameManifest $snapshotRolandBefore $snapshotRolandAfter)
  $briefSame=$briefHash -eq (Get-FileHash -Algorithm SHA256 -LiteralPath $Brief).Hash.ToLowerInvariant()
  $sha=(& git -C $here rev-parse HEAD).Trim()
  $ok=($steps.Count -eq 9 -and @($steps.Values | Where-Object { $_ -ne 0 }).Count -eq 0 -and $localSame -and $remoteSame -and $snapshotSame -and $snapshotMatches -and $briefSame)
  [pscustomobject]@{ok=$ok; commit=$sha; steps=$steps; flaggUnchanged=$localSame; rolandUnchanged=$remoteSame; snapshotUnchanged=$snapshotSame; snapshotSlot4MatchesLive=$snapshotMatches; briefUnchanged=$briefSame; inputs=@{snapshot=[IO.Path]::GetFullPath($Snapshot); snapshotManifestSha256=$snapshotHashes; brief=[IO.Path]::GetFullPath($Brief); briefSha256=$briefHash}; evidence=$Output} | ConvertTo-Json -Depth 6 | Set-Content -Encoding utf8 $verdictPath
}
if (!$ok) { exit 1 }
