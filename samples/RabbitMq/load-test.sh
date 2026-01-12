#!/bin/bash

# Brutal Load Test for RabbitMQ SDK
# Sends 500 messages as fast as possible

echo "🔥 BRUTAL LOAD TEST STARTING 🔥"
echo "Target: http://localhost:5000/publish"
echo "Messages: 500"
echo "================================"

API_URL="http://localhost:5000/publish"
TOTAL_MESSAGES=500
SUCCESS=0
FAILED=0

START_TIME=$(date +%s)

for i in $(seq 1 $TOTAL_MESSAGES); do
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL" \
        -H "Content-Type: application/json" \
        -d "{
            \"Id\": \"$(uuidgen)\",
            \"Content\": \"Load test message #$i\",
            \"Timestamp\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"
        }")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    
    if [ "$HTTP_CODE" = "200" ]; then
        ((SUCCESS++))
        echo -ne "\r✅ Sent: $i | Success: $SUCCESS | Failed: $FAILED"
    else
        ((FAILED++))
        echo -ne "\r❌ Sent: $i | Success: $SUCCESS | Failed: $FAILED"
    fi
done

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
echo "================================"
echo "🎯 LOAD TEST COMPLETE 🎯"
echo "Total Messages: $TOTAL_MESSAGES"
echo "✅ Success: $SUCCESS"
echo "❌ Failed: $FAILED"
echo "⏱️  Duration: ${DURATION}s"
echo "📊 Rate: $((TOTAL_MESSAGES / DURATION)) msg/s"
echo "================================"
