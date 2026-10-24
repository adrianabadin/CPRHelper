#!/usr/bin/env bash
# Keeps a free-tier Supabase project active by running one lightweight real
# database query through the REST API (PostgREST). Meant to run from cron.
#
# Config (never commit real values): an env file with
#   SUPABASE_URL=https://<project-ref>.supabase.co
#   SUPABASE_ANON_KEY=<anon / publishable key>
# Default path: /etc/supabase-keepalive.env (override with KEEPALIVE_ENV_FILE).
#
# Exit codes: 0 = query reached the database (HTTP 200), 1 = config error,
#             2 = all attempts failed.
set -u

ENV_FILE="${KEEPALIVE_ENV_FILE:-/etc/supabase-keepalive.env}"
TABLE="${KEEPALIVE_TABLE:-profiles}"
ATTEMPTS=3

log() { printf '%s %s\n' "$(date -u '+%Y-%m-%dT%H:%M:%SZ')" "$*"; }

if [ ! -r "$ENV_FILE" ]; then
  log "ERROR config file not readable: $ENV_FILE"
  exit 1
fi
# shellcheck disable=SC1090
. "$ENV_FILE"

if [ -z "${SUPABASE_URL:-}" ] || [ -z "${SUPABASE_ANON_KEY:-}" ]; then
  log "ERROR SUPABASE_URL and SUPABASE_ANON_KEY must be set in $ENV_FILE"
  exit 1
fi

URL="${SUPABASE_URL%/}/rest/v1/${TABLE}?select=id&limit=1"

for i in $(seq 1 "$ATTEMPTS"); do
  code=$(curl -sS -o /dev/null -w '%{http_code}' --max-time 20 \
    -H "apikey: ${SUPABASE_ANON_KEY}" \
    -H "Authorization: Bearer ${SUPABASE_ANON_KEY}" \
    "$URL" 2>/dev/null)
  if [ "$code" = "200" ]; then
    log "OK keep-alive query on '${TABLE}' (HTTP 200, attempt $i)"
    exit 0
  fi
  log "WARN attempt $i failed (HTTP ${code:-none})"
  [ "$i" -lt "$ATTEMPTS" ] && sleep 30
done

log "ERROR keep-alive failed after $ATTEMPTS attempts; check whether the project is paused"
exit 2
