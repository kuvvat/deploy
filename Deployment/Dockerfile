FROM ubuntu:latest

ENV DEBIAN_FRONTEND=noninteractive 

RUN apt-get update && apt-get install -y \
	git \
	software-properties-common \
	apt-transport-https \
	wget \
	python3-pip

RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb

ENV DEBIAN_FRONTEND=noninteractive 

RUN dpkg -i packages-microsoft-prod.deb

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update && apt-get install -y \
	dotnet-sdk-3.1

RUN git clone https://github.com/wassim-azirar/deploy.git /deploy

WORKDIR /deploy

RUN sed -i '10,43d' requirements.txt

RUN pip3 install -r requirements.txt

RUN sed -i 's/"ConnectionString": "host=localhost;port=5432;database=dev;user id=postgres;password=123456;pooling=true"/"ConnectionString": "host=172.15.0.3;port=5432;database=dev;user id=postgres;password=123456;pooling=true"/g' ./Api/appsettings.json

RUN dotnet restore

RUN dotnet build --configuration Release

WORKDIR ./Api/bin/Release/netcoreapp3.1
