param(
    [string]$projectDir,
    [string]$configuration,
    [string]$platform
)

# Define file urls
$x86Url = "https://cfhcable.dl.sourceforge.net/project/moonpdf/MoonPdf-0.3.0/MoonPdfLib-0.3.0-x86.zip"

# Define local paths
$moonPdfLibPath = Join-Path $projectDir "MoonPdfLib"
$dllPath = Join-Path $moonPdfLibPath "MoonPdfLib.dll"
$zipPath = Join-Path $moonPdfLibPath "MoonPdfLib.zip"

# Create x86 folder if not exists and download/extract files
if (!(Test-Path $dllPath)) {
    Invoke-WebRequest -Uri $x86Url -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath $moonPdfLibPath
    Move-Item -Path "$moonPdfLibPath\MoonPdfLib-0.3.0-x86\*.dll" -Destination $moonPdfLibPath
    Remove-Item -Recurse -Force "$moonPdfLibPath\MoonPdfLib-0.3.0-x86"
    Remove-Item $zipPath
}

# Determine which platform is being built and copy the appropriate files to the output directory
$outputPath = Join-Path $projectDir "bin\$configuration"
Copy-Item -Path "$moonPdfLibPath\*.dll" -Destination $outputPath -Recurse -Force