FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY ./src/poshtar/*.csproj ./
RUN dotnet restore

COPY ./src/poshtar .

ARG Version=0.0.0
RUN dotnet publish /p:Version=$Version -c Release -o /out --no-restore && rm -r *

COPY ./src/client-app/ ./
RUN npm ci && npm exec --vue-tsc && npm exec -- vite build --outDir /out/client-app

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

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
VOLUME ["/data"]
ENTRYPOINT ["dotnet", "poshtar.dll"]