#!/usr/bin/env bash
# Deletes the local SQLite database. It is recreated, empty, on the next run.
set -e
cd "$(dirname "$0")/src/Rpn.Web"
rm -f rpn.db rpn.db-shm rpn.db-wal
echo "Database removed. It will be recreated on the next 'dotnet run'."
