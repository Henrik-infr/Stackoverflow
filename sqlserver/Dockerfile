FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Install curl/gnupg, add Microsoft repo, then install Full-Text Search
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg && \
    curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list > /etc/apt/sources.list.d/mssql-server-2022.list && \
    apt-get update && \
    apt-get install -y mssql-server-fts && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

USER mssql
