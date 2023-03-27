apt-get update && sudo apt-get install -y --no-install-recommends \
    ca-certificates \
    ssl-cert \
    dovecot-core \
    dovecot-imapd \
    dovecot-mysql \
    dovecot-pgsql \
    dovecot-sqlite \

update-rc.d dovecot disable
rm -rf /var/lib/apt/lists/* /etc/dovecot