# Supabase keep-alive

Free-tier Supabase projects are paused after a period without activity. This
script runs one lightweight real database query (`GET /rest/v1/profiles?select=id&limit=1`)
so the project stays active. Schedule it with cron on an always-on server (the VPS);
GitHub Actions schedules are disabled after 60 days of repository inactivity, so
they are not a reliable host for this.

With Row Level Security the anon key usually gets an empty list (`[]`); the query
still reaches Postgres, which is what counts as activity.

## Install on the VPS

```bash
# 1. Copy the script (from a clone of this repo, or download it)
sudo install -m 755 tools/supabase-keepalive/keepalive.sh /usr/local/bin/supabase-keepalive

# 2. Create the config file (only root can read it)
sudo tee /etc/supabase-keepalive.env > /dev/null <<'EOF'
SUPABASE_URL=https://<project-ref>.supabase.co
SUPABASE_ANON_KEY=<anon key>
EOF
sudo chmod 600 /etc/supabase-keepalive.env

# 3. Run it once by hand; expect "OK keep-alive query ... (HTTP 200 ...)"
sudo /usr/local/bin/supabase-keepalive

# 4. Schedule it every 2 days at 03:17 UTC, logging to /var/log
echo '17 3 */2 * * root /usr/local/bin/supabase-keepalive >> /var/log/supabase-keepalive.log 2>&1' \
  | sudo tee /etc/cron.d/supabase-keepalive > /dev/null
sudo chmod 644 /etc/cron.d/supabase-keepalive
```

`SUPABASE_URL` and `SUPABASE_ANON_KEY` are the same values the app uses
(`Supabase:Url` / `Supabase:AnonKey`), also shown in the Supabase dashboard under
Project Settings → API.

## Check it

```bash
tail -n 5 /var/log/supabase-keepalive.log
```

Exit codes: `0` OK, `1` missing/unreadable config, `2` all three attempts failed
(the project may already be paused: restore it from the Supabase dashboard, then
run the script again).

Optional overrides: `KEEPALIVE_ENV_FILE` (config path), `KEEPALIVE_TABLE`
(table to query, default `profiles`).
