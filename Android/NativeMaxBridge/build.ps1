param(
    [string]$UnityEditorPath = 'C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor',
    [string]$WorkDirectory = 'D:\UserData\Administrator\Temp\PlayingCard-NativeMaxBridge'
)

$ErrorActionPreference = 'Stop'
$env:TEMP = 'D:\UserData\Administrator\Temp'
$env:TMP = $env:TEMP
$env:GRADLE_USER_HOME = Join-Path $WorkDirectory 'gradle-cache'
$moduleDirectory = $PSScriptRoot
$repositoryDirectory = [IO.Path]::GetFullPath((Join-Path $moduleDirectory '..\..'))
$WorkDirectory = [IO.Path]::GetFullPath($WorkDirectory)
if ($repositoryDirectory -notlike 'D:\*' -or $WorkDirectory -notlike 'D:\*') {
    throw 'The repository and build output must be on drive D.'
}
Set-Location -LiteralPath $moduleDirectory
New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null

$androidPlayer = Join-Path $UnityEditorPath 'Data\PlaybackEngines\AndroidPlayer'
$javac = Join-Path $androidPlayer 'OpenJDK\bin\javac.exe'
$jar = Join-Path $androidPlayer 'OpenJDK\bin\jar.exe'
$androidJar = Join-Path $androidPlayer 'SDK\platforms\android-34\android.jar'
$unityJar = Join-Path $androidPlayer 'Variations\il2cpp\Release\Classes\classes.jar'
$vendorAar = Join-Path $repositoryDirectory 'Assets\Plugins\Android\sdk-2.0.6.u-del789.top.aar'
foreach ($requiredFile in @($javac, $jar, $androidJar, $unityJar, $vendorAar)) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw ('Required build input is missing: ' + $requiredFile)
    }
}

$maxAar = Join-Path $WorkDirectory 'applovin-sdk-13.6.3.aar'
if (-not (Test-Path -LiteralPath $maxAar)) {
    Invoke-WebRequest -Uri 'https://repo.maven.apache.org/maven2/com/applovin/applovin-sdk/13.6.3/applovin-sdk-13.6.3.aar' -OutFile $maxAar
}
$expectedMaxHash = '384745BA1952F0F516FEEECA67D64EDF346D2CCC577FA2C1BCD3F0CF3EA30B33'
if ((Get-FileHash -LiteralPath $maxAar -Algorithm SHA256).Hash -ne $expectedMaxHash) {
    throw 'The downloaded MAX 13.6.3 AAR differs from the verified build input.'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Export-ClassesJar([string]$ArchivePath, [string]$JarPath) {
    $archive = [IO.Compression.ZipFile]::OpenRead($ArchivePath)
    try {
        $entry = $archive.GetEntry('classes.jar')
        if ($null -eq $entry) { throw ('classes.jar is missing from ' + $ArchivePath) }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $JarPath, $true)
    }
    finally { $archive.Dispose() }
}

$maxJar = Join-Path $WorkDirectory 'applovin-sdk-13.6.3.jar'
$reportingJar = Join-Path $WorkDirectory 'sndorun-2.0.6.jar'
Export-ClassesJar $maxAar $maxJar
Export-ClassesJar $vendorAar $reportingJar

$buildDirectory = Join-Path $WorkDirectory ([guid]::NewGuid().ToString('N'))
$classesDirectory = Join-Path $buildDirectory 'classes'
$archiveDirectory = Join-Path $buildDirectory 'aar'
New-Item -ItemType Directory -Path $classesDirectory,$archiveDirectory -Force | Out-Null
$source = Join-Path $moduleDirectory 'src\main\java\com\citydaheng\solitaire\ads\NativeMaxBridge.java'
$classPath = @($androidJar, $unityJar, $maxJar, $reportingJar) -join [IO.Path]::PathSeparator
& $javac '-J-Djava.io.tmpdir=D:\UserData\Administrator\Temp' -encoding UTF-8 -source 11 -target 11 -classpath $classPath -d $classesDirectory $source
if ($LASTEXITCODE -ne 0) { throw 'Native MAX bridge Java compilation failed.' }

$classesJar = Join-Path $archiveDirectory 'classes.jar'
& $jar cf $classesJar -C $classesDirectory .
if ($LASTEXITCODE -ne 0) { throw 'Could not package the compiled bridge classes.' }
Copy-Item -LiteralPath (Join-Path $moduleDirectory 'src\main\AndroidManifest.xml') -Destination $archiveDirectory
Copy-Item -LiteralPath (Join-Path $moduleDirectory 'consumer-rules.pro') -Destination (Join-Path $archiveDirectory 'proguard.txt')
[IO.File]::WriteAllText((Join-Path $archiveDirectory 'R.txt'), '')
$outputAar = Join-Path $repositoryDirectory 'Assets\Plugins\Android\native-max-bridge.aar'
$temporaryAar = Join-Path $buildDirectory 'native-max-bridge.aar'
[IO.Compression.ZipFile]::CreateFromDirectory($archiveDirectory, $temporaryAar)
Copy-Item -LiteralPath $temporaryAar -Destination $outputAar -Force
Write-Output ('Built ' + $outputAar)
