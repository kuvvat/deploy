wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1

sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -
sudo apt-get update
sudo apt-get -y install postgresql

sudo -u postgres psql
create database prod;
alter user postgres password '123456';
grant all privileges on database prod to postgres;
\q

git clone https://github.com/wassim-azirar/deploy.git
cd deploy/
dotnet restore
dotnet build --configuration Release
cd Api/bin/Release/netcoreapp3.1/
dotnet Api.dll

sudo apt update
yes Y | sudo apt install nginx

sudo apt-get update
sudo apt-get install software-properties-common
sudo add-apt-repository universe
sudo add-apt-repository ppa:certbot/certbot
sudo apt-get update
yes Y | sudo apt-get install certbot python-certbot-nginx
sudo certbot --nginx

sudo nano /etc/nginx/sites-available/default
sudo nginx -t
sudo service nginx reload