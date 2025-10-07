#!/usr/bin/env bash
mkdir -p exd-data
docker run -v "$PWD/exd-data":/app/exd-data:z ghcr.io/workingrobot/ffxiv-downloader:latest download --files '^sqpack\/ffxiv\/0a0000\..+$' -o /app/exd-data