FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dotnet-build
WORKDIR /app
COPY ./src/poshtar/*.csproj .
RUN dotnet restore
COPY ./src/poshtar .
ARG Version=0.0.0
RUN dotnet publish /p:Version=$Version -c Release -o /out --no-restore

FROM node:19 AS node-build
WORKDIR /app
COPY ./src/client-app/ .
RUN npm ci && npm exec --vue-tsc && npm exec -- vite build --outDir /out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=dotnet-build /out ./
COPY --from=node-build /out ./client-app

ENV LC_ALL C
ARG DEBIAN_FRONTEND=noninteractive
RUN set -eux; \
    apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    ssl-cert \
    dovecot-core \
    dovecot-imapd \
    dovecot-mysql \
    dovecot-pgsql \
    dovecot-sqlite \
    ; \
    groupadd -g 1000 vmail && \
    useradd -u 1000 -g 1000 vmail --shell /usr/sbin/nologin && \
    passwd -l vmail \
    ; \
    update-rc.d dovecot disable && \
    rm -rf /var/lib/apt/lists /etc/dovecot && \
    echo "alias ll='ls -hal'" >> ~/.bashrc;

ENV ASPNETCORE_URLS=http://*:5080 \
    LOCALE=en-US \
    TZ=America/Chicago

VOLUME ["/data/certs", "/data/config", "/data/logs", "/data/mail"]
EXPOSE 5025/TCP 5587/TCP 5993/TCP 5080/TCP
ENTRYPOINT ["dotnet", "poshtar.dll"]