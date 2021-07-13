#!/bin/sh
cd /home/ubuntu/netcore
rm -rf ./nginx/ssl/*
cp /etc/letsencrypt/live/nador.app/fullchain.pem ./nginx/ssl/
cp /etc/letsencrypt/live/nador.app/privkey.pem ./nginx/ssl/
docker-compose start web
