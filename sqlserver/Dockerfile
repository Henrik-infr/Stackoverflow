FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update && \
    apt-get install -y mssql-server-fts && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

USER mssql
