# Deploy

## Install .NET Core 3.1

`wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb`

`sudo dpkg -i packages-microsoft-prod.deb`

`sudo apt update`

`sudo apt install apt-transport-https -y`

`sudo apt install dotnet-sdk-3.1 `

## Install python

`sudo apt update`

`sudo apt install software-properties-common`

`sudo add-apt-repository ppa:deadsnakes/ppa`

`sudo apt install python3.7`

## Install PiP

`sudo apt update`

`sudo apt install python3-pip`

## Install Python libraries

`pip install -r requirements.txt`

## Clone repository

`git clone https://github.com/wassim-azirar/deploy.git`

`cd deploy`

`dotnet restore`

`dotnet build --configuration Release`

`cd .\Api\bin\Release\netcoreapp3.1\`

`dotnet .\Api.dll`

The application will be available at http://localhost:5001

If you get this error `password authentication failed for user "postgres"`

    1) Update ConnectionString in Api\appsettings.Production.json
    2) run again dotnet build --configuration Release
    3) go to .\Api\bin\Release\netcoreapp3.1\
    4) dotnet .\Api.dll
