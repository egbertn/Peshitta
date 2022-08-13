#!/snap/bin/powershell
#Custom build if not using Azure
#### creates a build for Windows x64 especially for etisalat
#do not overwrite appsettings.json on the target machine
$publishfolder = './publish'

$version = '1.0.1'

dotnet build . --runtime linux-arm  --self-contained true  -c Release /p:PublishSingleFile=true -o ./app/build -p:SourceRevisionId=$version
dotnet publish . --runtime linux-arm --self-contained true --no-restore -c Release /p:PublishSingleFile=true -o $publishfolder
# scp index.html adc@192.168.1.64:/var/www/html/peshitta.nl/wwwroot
