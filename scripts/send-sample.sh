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
  -d "{\"deviceId\":\"$DEVICE_ID\",\"value\":$VALUE,\"timestamp\":$TIMESTAMP,\"nonce\":\"$NONCE\",\"signature\":\"$signature\"}" \
  -D /tmp/sensorplatform_headers.txt \
  -o /tmp/sensorplatform_body.txt

status_code="$(awk 'toupper($1) ~ /^HTTP\// { code=$2 } END { print code }' /tmp/sensorplatform_headers.txt)"
content_type="$(awk -F': ' 'tolower($1) == "content-type" { print tolower($2) }' /tmp/sensorplatform_headers.txt | tr -d '\r' | tail -n 1)"

echo "HTTP status: ${status_code:-unknown}"

if [[ "${content_type}" == application/json* ]] || [[ "${content_type}" == *"+json"* ]]; then
  jq . /tmp/sensorplatform_body.txt
else
  cat /tmp/sensorplatform_body.txt
fi
