# Deploy — development / demo server

Runs on the chromebox in the same shape as `~/casa-illimani-deploy`: images are built by CI on
GitHub-hosted runners and pushed to GHCR, the self-hosted runner only pulls and restarts, and the
Cloudflare Tunnel is a sidecar inside the stack.

```
Cloudflare edge (TLS ends here)
        │
        ▼
  cloudflared  ──► http://web:80        (all inside the stack network)
  web          nginx, LAN :8081
               ├── /         → SPA bundle, history fallback
               ├── /api/…    → proxy_pass http://api:8080
               └── /healthz  → proxy_pass http://api:8080/health
  api          NsStore.Api :8080, no published port
  postgres     :5432, volume pgdata
```

`docker-compose.prod.yml` is versioned here and copied to `~/ns-store-deploy/` by the ns-store
deploy job. The `.env` beside it is **server-only and hand-maintained** — it holds the tunnel token.

## Why one origin

The refresh token is an httpOnly cookie scoped to `/api/v1/auth`, issued with `Secure` and
`SameSite=Strict` (`src/NsStore.Api/Endpoints/AuthEndpoints.cs`). Serving the SPA and the API from
a single origin over HTTPS is what makes it work — the same arrangement the Vite dev proxy creates
locally. The tunnel supplies the HTTPS half.

**A browser will not store a `Secure` cookie over plain HTTP**, so reaching the app directly at
`http://192.168.0.130:8081` lets you log in but drops the session on the next refresh. The tunnel
hostname is the only supported entry point; the LAN port is for smoke tests.

## First-time setup

1. Create a tunnel for this stack in the Cloudflare Zero Trust dashboard with a Public Hostname
   pointing at `http://web:80`, and put its token in `~/ns-store-deploy/.env`.
2. Fill the rest of `~/ns-store-deploy/.env` from `.env.example` in this folder.
3. Push to `master` — CI builds the API image and the deploy job takes it from there. Push
   ns-store-ui's `main` for the web image. **The branches differ between the two repos.**

No GitHub Secrets are needed: images come from GHCR with the built-in `GITHUB_TOKEN`, and every
secret value lives in the server-side `.env`.

## Deploying

A normal deploy **never touches existing data**: the demo seeder no-ops as soon as the catalog has
rows, so whatever the client entered while testing survives.

Ownership is split so neither repo needs the other's files: **ns-store owns the compose file** and
restarts the whole stack; **ns-store-ui only pulls and restarts `web`**.

## Demo dataset

Six months of history for a laptop-parts store in Cochabamba: 2 branches, 140 products across 18
categories, 25 clients, 44 purchases, 200 sales (about a quarter on credit, with instalments and
open overdue balances), 6 inter-branch transfers, adjustments, orders and quotations.

Seller logins are `mquispe`, `jvargas` and `dcamacho`, all with password `Demo1234`.

To rebuild it, run the **Reset demo data** workflow in ns-store and type `RESET` in the confirmation
input. That is the only path that deletes anything; it keeps the admin account and the business
settings. The dataset comes from a fixed random seed, so a reset reproduces the same numbers.

## Operating notes

- The API publishes no host port. To reach it:
  `docker compose -f docker-compose.prod.yml exec web wget -qO- http://api:8080/health`
- `ForwardedHeaders__Enabled=true` is what lets the API see the caller's real address, which the
  login rate limiter partitions on. Without it every user shares one 10-per-minute bucket. It is
  only safe while nginx is the sole route to the API — do not publish a port for it.
- Logs: `cd ~/ns-store-deploy && docker compose -f docker-compose.prod.yml logs -f api`
- Set `ASPNETCORE_ENVIRONMENT=Development` in the `.env` to expose the Scalar API explorer at
  `/scalar/v1`.
- LAN ports already taken on this box: 8080 (qbittorrent), 8090 (service-center), 8888 (nextcloud).
