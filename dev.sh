# to install dependencies follow Dockerfile
sudo apt-get update && sudo apt-get install -y --no-install-recommends \
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
	sudo rm -rf /var/lib/apt/lists/*

# monitoring


# postfix 
sudo postfix stop && sudo postfix -c /workspaces/poshtar/src/poshtar/data/config/postfix start
tail /var/log/postfix.log -f

# dovecot
sudo dovecot stop; sudo dovecot -c /workspaces/poshtar/src/poshtar/data/config/dovecot/dovecot.conf
tail /var/log/dovecot.log -f

openssl s_client -quiet -crlf -connect localhost:993
openssl s_client -quiet -crlf -connect localhost:993 -starttls imap