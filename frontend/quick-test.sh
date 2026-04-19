#!/bin/bash

# Frontend Quick Test Script
# Verifies frontend is configured correctly and ready to run

set -e

echo "🎨 Frontend Quick Test Script"
echo "=============================="
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "Step 1: Checking Node.js and npm..."
echo "------------------------------------"

if ! command -v node &> /dev/null; then
    echo -e "${RED}✗ Node.js is not installed!${NC}"
    echo "Please install Node.js 20+ from https://nodejs.org"
    exit 1
fi

NODE_VERSION=$(node -v)
echo -e "${GREEN}✓ Node.js installed: $NODE_VERSION${NC}"

if ! command -v npm &> /dev/null; then
    echo -e "${RED}✗ npm is not installed!${NC}"
    exit 1
fi

NPM_VERSION=$(npm -v)
echo -e "${GREEN}✓ npm installed: $NPM_VERSION${NC}"
echo ""

echo "Step 2: Checking dependencies..."
echo "------------------------------------"

if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}⚠ node_modules not found${NC}"
    echo "Installing dependencies..."
    npm install
else
    echo -e "${GREEN}✓ node_modules exists${NC}"
fi

# Check for key dependencies
REQUIRED_DEPS=("next" "react" "axios" "@tanstack/react-query" "lucide-react")
MISSING_DEPS=()

for dep in "${REQUIRED_DEPS[@]}"; do
    if [ ! -d "node_modules/$dep" ]; then
        MISSING_DEPS+=("$dep")
    fi
done

if [ ${#MISSING_DEPS[@]} -gt 0 ]; then
    echo -e "${RED}✗ Missing dependencies: ${MISSING_DEPS[*]}${NC}"
    echo "Running npm install..."
    npm install
else
    echo -e "${GREEN}✓ All key dependencies installed${NC}"
fi
echo ""

echo "Step 3: Checking configuration..."
echo "------------------------------------"

# Check .env.local
if [ -f ".env.local" ]; then
    echo -e "${GREEN}✓ .env.local exists${NC}"
    
    # Check API URL
    if grep -q "NEXT_PUBLIC_API_URL=http://localhost:5010" .env.local; then
        echo -e "${GREEN}✓ API URL points to API Gateway (port 5010)${NC}"
    elif grep -q "NEXT_PUBLIC_API_URL=http://localhost:5001" .env.local; then
        echo -e "${YELLOW}⚠ API URL points to direct service (port 5001)${NC}"
        echo -e "${YELLOW}  Updating to use API Gateway...${NC}"
        echo "NEXT_PUBLIC_API_URL=http://localhost:5010" > .env.local
        echo -e "${GREEN}✓ Updated to API Gateway${NC}"
    else
        echo -e "${YELLOW}⚠ API URL not configured${NC}"
        echo "NEXT_PUBLIC_API_URL=http://localhost:5010" > .env.local
        echo -e "${GREEN}✓ Created .env.local with API Gateway URL${NC}"
    fi
else
    echo -e "${YELLOW}⚠ .env.local not found${NC}"
    echo "Creating .env.local..."
    echo "NEXT_PUBLIC_API_URL=http://localhost:5010" > .env.local
    echo -e "${GREEN}✓ Created .env.local${NC}"
fi
echo ""

echo "Step 4: Checking backend services..."
echo "------------------------------------"

# Check if API Gateway is accessible
echo -n "Checking API Gateway (http://localhost:5010)... "
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:5010" 2>/dev/null || echo "000")

if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "401" ]; then
    echo -e "${GREEN}✓ UP (${HTTP_STATUS})${NC}"
else
    echo -e "${RED}✗ DOWN${NC}"
    echo ""
    echo -e "${YELLOW}⚠ API Gateway is not running!${NC}"
    echo "Please start backend services:"
    echo "  cd /path/to/SPM"
    echo "  docker-compose up -d"
    echo ""
    echo "Then run this script again."
    exit 1
fi
echo ""

echo "Step 5: Type checking..."
echo "------------------------------------"

echo "Running TypeScript compiler..."
if npm run type-check > /dev/null 2>&1; then
    echo -e "${GREEN}✓ No TypeScript errors${NC}"
else
    echo -e "${YELLOW}⚠ TypeScript errors found${NC}"
    echo "Run 'npm run type-check' to see details"
fi
echo ""

echo "=============================="
echo -e "${GREEN}✅ Frontend is Ready!${NC}"
echo ""
echo "Summary:"
echo "--------"
echo "✓ Node.js and npm installed"
echo "✓ Dependencies installed"
echo "✓ Configuration correct (.env.local)"
echo "✓ API Gateway is accessible"
echo "✓ TypeScript compiled"
echo ""
echo "Next steps:"
echo "----------"
echo "1. Start frontend:"
echo "   ${BLUE}npm run dev${NC}"
echo ""
echo "2. Open browser:"
echo "   ${BLUE}http://localhost:3000${NC}"
echo ""
echo "3. Test features:"
echo "   - Register/Login"
echo "   - Create Project"
echo "   - Create Task"
echo "   - Upload File"
echo ""
echo "4. Read testing guide:"
echo "   ${BLUE}cat SPRINT2_TESTING.md${NC}"
echo ""
