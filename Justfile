# NsStore API — single command surface.
# Run `just` for the verification loop, `just help` for everything else.
# No sh on PATH on the dev machine, so recipes run through PowerShell.

set windows-shell := ["powershell.exe", "-NoLogo", "-NonInteractive", "-Command"]

# Debug locally so iteration is fast; CI sets CONFIGURATION=Release to test what it ships.
config := env_var_or_default("CONFIGURATION", "Debug")

default: check

# list every target
help:
    @just --list

# If it ever crosses 60s, split the slow part out rather than running it less often: under a
# minute an agent iterates, over four it runs once and guesses the rest. ~16s on 2026-09-08.
# work is not done until this is green: build + every test + the migration guard
check: build test schema

build:
    dotnet build --nologo -c {{ config }}

# --no-build: check runs build first, so this stays fast on repeated iterations
test: build
    dotnet test --no-build --nologo -c {{ config }}

# fails when an entity or an IEntityTypeConfiguration changed without a migration
schema: tools
    dotnet ef migrations has-pending-model-changes --project src/NsStore.Infrastructure --startup-project src/NsStore.Api

# create a migration: just migration AddSomething
migration name: tools
    dotnet ef migrations add {{ name }} --project src/NsStore.Infrastructure --startup-project src/NsStore.Api --output-dir Persistence/Migrations

# apply migrations to the local database
migrate-db: tools
    dotnet ef database update --project src/NsStore.Infrastructure --startup-project src/NsStore.Api

# restore the pinned local tools (dotnet-ef); cheap and idempotent
tools:
    @dotnet tool restore --verbosity quiet

# Postgres only — appsettings.Development.json points at it
db:
    docker compose up -d db

# the API on the launchSettings port
run:
    dotnet run --project src/NsStore.Api

# full stack; needs .env with JWT_SIGNING_KEY set
up:
    docker compose up --build
