# Deploy — development / demo server

The stack runs on the chromebox as two independent Compose projects joined by an external Docker
network, so either repository can redeploy without touching the other's files.

```
Cloudflare Tunnel (TLS ends here)
        │  http
        ▼
  web   nginx:alpine, :8081        [ns-store-ui]
        ├── /            → SPA bundle, history fallback
        ├── /api/…       → proxy_pass http://api:8080
        └── /healthz     → proxy_pass http://api:8080/health
        │
        ├──────── nsstore-net (external) ────────┐
                                                 │
  api   NsStore.Api :8080, no published port    [ns-store]
  db    postgres:17-alpine, volume db-data      [ns-store]
```

## Why nginx and why one origin

The refresh token is an httpOnly cookie scoped to `/api/v1/auth`, issued with `Secure` and
`SameSite=Strict` (`src/NsStore.Api/Endpoints/AuthEndpoints.cs`). Serving the SPA and the API from
one origin over HTTPS is what makes it behave — it is the same arrangement the Vite dev proxy
creates locally. The tunnel supplies the HTTPS half.

**A browser will not store a `Secure` cookie over plain HTTP**, so reaching the app directly at
`http://192.168.0.130:8081` lets you log in but drops the session on the next refresh. The tunnel
hostname is the only supported entry point.

## First-time setup on the server

1. Create the shared network (the deploy jobs also do this, idempotently):

   ```bash
   docker network create nsstore-net
   ```

2. Add these repository secrets to **ns-store** on GitHub:

   | Secret | Notes |
   |---|---|
   | `POSTGRES_PASSWORD` | any strong value |
   | `JWT_SIGNING_KEY` | 32+ chars, `openssl rand -base64 48` |
   | `PUBLIC_ORIGIN` | the tunnel hostname, e.g. `https://nsstore-dev.example.com` |
   | `SEED_ADMIN_USERNAME` | only used while no user exists |
   | `SEED_ADMIN_PASSWORD` | idem |

   The deploy job writes them to `~/.nsstore/deploy.env` (mode 600) on the runner. `.env.example`
   in this folder documents the same keys for a manual run.

3. Point the Cloudflare Tunnel's ingress for the public hostname at `http://localhost:8081`.

## Deploying

Push to `master` (ns-store) or `main` (ns-store-ui) — note the branches differ. Each repo's CI runs
first and the deploy job only follows on a push to that branch.

A normal deploy **never touches existing data**: the demo seeder no-ops as soon as the catalog has
rows, so whatever the client entered while testing survives.

## Demo dataset

Six months of history for a laptop-parts store in Cochabamba: 2 branches, 140 products across 18
categories, 25 clients, 44 purchases, 200 sales (about a quarter on credit, with instalments and
open overdue balances), 6 inter-branch transfers, adjustments, orders and quotations.

Seller logins are `mquispe`, `jvargas` and `dcamacho`, all with password `Demo1234`.

To rebuild it from scratch, run the **Reset demo data** workflow in ns-store and type `RESET` in the
confirmation input. That is the only path that deletes anything; it keeps the admin account and the
business settings. The dataset is generated from a fixed random seed, so a reset reproduces exactly
the same numbers.

## Operating notes

- The API publishes no host port. To reach it directly:
  `docker run --rm --network nsstore-net curlimages/curl -fsS http://api:8080/health`
- `ForwardedHeaders__Enabled=true` is what lets the API see the caller's real address, which the
  login rate limiter partitions on. Without it every user shares one 10-per-minute bucket. It is
  only safe because the API is unreachable except through nginx — do not publish a port for it.
- Logs: `docker compose -f deploy/docker-compose.deploy.yml --env-file ~/.nsstore/deploy.env logs -f api`
- Set `ASPNETCORE_ENVIRONMENT=Development` in the env file to expose the Scalar API explorer at
  `/scalar/v1`.
