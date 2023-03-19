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

RUN set -eux; \
    apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    ssl-cert \
    postfix \
    postfix-pgsql \
    postfix-mysql \
    postfix-sqlite \
    dovecot-core \
    dovecot-imapd \
    dovecot-lmtpd \
    dovecot-managesieved \
    dovecot-mysql \
    dovecot-pgsql \
    dovecot-sieve \
    dovecot-sqlite \
    ; \
	rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://*:50505 \
    LOCALE=en-US \
    TZ=America/Chicago

EXPOSE 50505
VOLUME ["/data/certs", "/data/config", "/data/logs", "/data/mail"]
ENTRYPOINT ["dotnet", "poshtar.dll"]