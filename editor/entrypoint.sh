#!/bin/sh
# Fix permissions on mounted rules directory so nginx can write
chmod -R a+rw /app/rules 2>/dev/null || true
exec "$@"
