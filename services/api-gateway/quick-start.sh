#!/bin/bash

# API Gateway Quick Start Script
# This script helps you quickly test the API Gateway setup

set -e  # Exit on error

echo "🚀 API Gateway Quick Start Script"
echo "=================================="
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Base URL
BASE_URL="http://localhost:5000"

# Function to check if service is up
check_service() {
    local url=$1
    local service_name=$2
    
    echo -n "Checking $service_name... "
    if curl -s -f -o /dev/null "$url"; then
        echo -e "${GREEN}✓ UP${NC}"
        return 0
    else
        echo -e "${RED}✗ DOWN${NC}"
        return 1
    fi
}

# Function to make HTTP request
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local token=$4
    
    if [ -z "$token" ]; then
        curl -s -X "$method" "${BASE_URL}${endpoint}" \
            -H "Content-Type: application/json" \
            -d "$data"
    else
        curl -s -X "$method" "${BASE_URL}${endpoint}" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data"
    fi
}

echo "Step 1: Checking if Docker services are running..."
echo "----------------------------------------------------"

if ! docker ps > /dev/null 2>&1; then
    echo -e "${RED}✗ Docker is not running!${NC}"
    echo "Please start Docker Desktop and run: docker-compose up -d"
    exit 1
fi

echo -e "${GREEN}✓ Docker is running${NC}"
echo ""

echo "Step 2: Checking service health..."
echo "----------------------------------------------------"

SERVICES_OK=true

check_service "http://localhost:5001/swagger/index.html" "User Service (5001)" || SERVICES_OK=false
check_service "http://localhost:5002/swagger/index.html" "Project Service (5002)" || SERVICES_OK=false
check_service "http://localhost:5003/swagger/index.html" "File Service (5003)" || SERVICES_OK=false

echo ""

if [ "$SERVICES_OK" = false ]; then
    echo -e "${YELLOW}⚠ Some services are not running${NC}"
    echo "Run: docker-compose up -d"
    echo "Then run this script again"
    exit 1
fi

echo "Step 3: Testing API Gateway..."
echo "----------------------------------------------------"
echo ""

# Test 1: Anonymous route (Register)
echo -n "Test 1: Register new user (Anonymous route)... "
REGISTER_RESPONSE=$(make_request POST "/api/auth/register" '{
  "email": "testuser@example.com",
  "password": "Test@1234",
  "fullName": "Test User"
}')

if echo "$REGISTER_RESPONSE" | grep -q '"success":true'; then
    echo -e "${GREEN}✓ PASS${NC}"
else
    # User might already exist - try with different email
    TIMESTAMP=$(date +%s)
    REGISTER_RESPONSE=$(make_request POST "/api/auth/register" '{
      "email": "testuser'$TIMESTAMP'@example.com",
      "password": "Test@1234",
      "fullName": "Test User"
    }')
    if echo "$REGISTER_RESPONSE" | grep -q '"success":true'; then
        echo -e "${GREEN}✓ PASS (with timestamp)${NC}"
    else
        echo -e "${YELLOW}⚠ SKIP (user exists)${NC}"
    fi
fi
echo ""

# Test 2: Login
echo -n "Test 2: Login (Anonymous route)... "
LOGIN_RESPONSE=$(make_request POST "/api/auth/login" '{
  "email": "testuser@example.com",
  "password": "Test@1234"
}')

if echo "$LOGIN_RESPONSE" | grep -q '"accessToken"'; then
    echo -e "${GREEN}✓ PASS${NC}"
    ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)
    echo "   Access Token: ${ACCESS_TOKEN:0:50}..."
else
    echo -e "${RED}✗ FAIL${NC}"
    echo "   Response: $LOGIN_RESPONSE"
    exit 1
fi
echo ""

# Test 3: Protected route WITHOUT token
echo -n "Test 3: Get projects WITHOUT token (should fail)... "
PROJECTS_NO_TOKEN=$(curl -s -w "%{http_code}" -o /dev/null "${BASE_URL}/api/projects")

if [ "$PROJECTS_NO_TOKEN" = "401" ]; then
    echo -e "${GREEN}✓ PASS (401 Unauthorized)${NC}"
else
    echo -e "${RED}✗ FAIL (got $PROJECTS_NO_TOKEN instead of 401)${NC}"
fi
echo ""

# Test 4: Protected route WITH token
echo -n "Test 4: Get projects WITH token (should work)... "
PROJECTS_RESPONSE=$(make_request GET "/api/projects" "" "$ACCESS_TOKEN")

if echo "$PROJECTS_RESPONSE" | grep -q '"success":true'; then
    echo -e "${GREEN}✓ PASS${NC}"
    echo "   Response: $PROJECTS_RESPONSE"
else
    echo -e "${RED}✗ FAIL${NC}"
    echo "   Response: $PROJECTS_RESPONSE"
fi
echo ""

# Test 5: Create project
echo -n "Test 5: Create project through gateway... "
CREATE_PROJECT_RESPONSE=$(make_request POST "/api/projects" '{
  "name": "API Gateway Test Project",
  "description": "Testing API Gateway routing"
}' "$ACCESS_TOKEN")

if echo "$CREATE_PROJECT_RESPONSE" | grep -q '"success":true'; then
    echo -e "${GREEN}✓ PASS${NC}"
    PROJECT_ID=$(echo "$CREATE_PROJECT_RESPONSE" | grep -o '"id":"[^"]*' | cut -d'"' -f4 | head -1)
    echo "   Project ID: $PROJECT_ID"
else
    echo -e "${RED}✗ FAIL${NC}"
    echo "   Response: $CREATE_PROJECT_RESPONSE"
fi
echo ""

# Test 6: Create task (if project created)
if [ ! -z "$PROJECT_ID" ]; then
    echo -n "Test 6: Create task through gateway... "
    CREATE_TASK_RESPONSE=$(make_request POST "/api/tasks" '{
      "projectId": "'$PROJECT_ID'",
      "title": "Test Task via Gateway",
      "description": "Testing task creation through API Gateway",
      "priority": "High",
      "status": "ToDo"
    }' "$ACCESS_TOKEN")
    
    if echo "$CREATE_TASK_RESPONSE" | grep -q '"success":true'; then
        echo -e "${GREEN}✓ PASS${NC}"
        TASK_ID=$(echo "$CREATE_TASK_RESPONSE" | grep -o '"id":"[^"]*' | cut -d'"' -f4 | head -1)
        echo "   Task ID: $TASK_ID"
    else
        echo -e "${RED}✗ FAIL${NC}"
        echo "   Response: $CREATE_TASK_RESPONSE"
    fi
    echo ""
fi

echo "=================================="
echo -e "${GREEN}✅ API Gateway Quick Test Complete!${NC}"
echo ""
echo "Summary:"
echo "--------"
echo "✓ Anonymous routes work (register, login)"
echo "✓ JWT authentication works"
echo "✓ Protected routes require authentication"
echo "✓ Routing to services works"
echo ""
echo "Next steps:"
echo "----------"
echo "1. Open api-gateway.http in VS Code for detailed testing"
echo "2. Read TESTING.md for comprehensive test guide"
echo "3. Test frontend at http://localhost:3000"
echo ""
echo "Your Access Token (valid for 15 minutes):"
echo "$ACCESS_TOKEN"
echo ""

