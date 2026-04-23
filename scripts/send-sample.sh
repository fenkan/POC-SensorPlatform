#!/usr/bin/env bash
set -euo pipefail

API_BASE_URL="${API_BASE_URL:-http://localhost:5143}"
DEVICE_ID="${DEVICE_ID:-sensor-temp-01}"
SECRET="${SECRET:-temp-01-secret}"
VALUE="${VALUE:-22.5}"
NONCE="${NONCE:-$(cat /proc/sys/kernel/random/uuid)}"
TIMESTAMP="${TIMESTAMP:-$(date +%s)}"

payload="${DEVICE_ID}|${VALUE}|${TIMESTAMP}|${NONCE}"
signature="$(printf '%s' "$payload" | openssl dgst -sha256 -hmac "$SECRET" -binary | openssl base64)"

cat <<EOF
Sending ingest payload
- deviceId: $DEVICE_ID
- value: $VALUE
- timestamp: $TIMESTAMP
- nonce: $NONCE
EOF

curl -sS -X POST "$API_BASE_URL/api/ingest" \
  -H 'Content-Type: application/json' \
  -d "{\"deviceId\":\"$DEVICE_ID\",\"value\":$VALUE,\"timestamp\":$TIMESTAMP,\"nonce\":\"$NONCE\",\"signature\":\"$signature\"}" | jq .
