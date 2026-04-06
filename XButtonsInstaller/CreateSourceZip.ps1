$staging = 'TempSource'
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging
Get-ChildItem -Path '$(SolutionDir)*' -Recurse | Where-Object { 
	$_.FullName -notmatch '\\(bin|obj|\.vs|\.git|TempSource)(\\|$)' -and 
	$_.Extension -notmatch '(\.user|\.suo)$' -and 
	$_.Name -ne 'src.zip' 
} | Copy-Item -Destination { Join-Path $staging $_.FullName.Substring('$(SolutionDir)'.Length) } -Force
Compress-Archive -Path \"$staging\*\" -DestinationPath '$(ProjectDir)src.zip' -Force
Remove-Item $staging -Recurse -Force