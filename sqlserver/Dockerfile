FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# The base image has no curl/wget and Ubuntu HTTP repos are blocked.
# Use apt with only the Microsoft HTTPS repo to install FTS.
# First, write the Microsoft repo config manually (no curl needed).
RUN echo "deb [arch=amd64,arm64,armhf signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/22.04/mssql-server-2022 jammy main" > /etc/apt/sources.list.d/mssql-server-2022.list && \
    echo "deb [arch=amd64,arm64,armhf signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" > /etc/apt/sources.list.d/microsoft-prod.list && \
    apt-get update -o Dir::Etc::sourcelist=/etc/apt/sources.list.d/mssql-server-2022.list -o Dir::Etc::sourceparts="-" -o APT::Get::List-Cleanup="0" && \
    apt-get update -o Dir::Etc::sourcelist=/etc/apt/sources.list.d/microsoft-prod.list -o Dir::Etc::sourceparts="-" -o APT::Get::List-Cleanup="0" && \
    apt-get install -y --no-install-recommends mssql-server-fts && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

USER mssql
