#/usr/bin/bash
#Custom build if not using Azure
#### creates a build for Windows x64 especially for etisalat
#do not overwrite appsettings.json on the target machine
$publishfolder='./publish'

$version='1.0.1'

dotnet build . --runtime linux-arm64   -c Release  -o ./app/build -p:SourceRevisionId=$version
dotnet publish . --runtime linux-arm64  --no-restore -c Release  -o  ~/projects/publish/peshitta.nl
# scp index.html adc@192.168.1.64:/var/www/html/peshitta.nl/wwwroot
