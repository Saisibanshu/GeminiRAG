# Versnn Deployment Guide

## Prerequisites

### GitHub Secrets Required

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

**Backend Secrets:**
- `DB_CONNECTION_STRING` - Your production SQL Server connection string
- `JWT_SECRET_KEY` - Strong random secret key (min 32 characters)
- `GEMINI_API_KEY` - Your Google Gemini API key
- `SERVER_HOST` - Your server IP/hostname (if deploying via SSH)
- `SERVER_USER` - SSH username
- `SERVER_SSH_KEY` - Private SSH key for deployment

**Frontend Secrets:**
- Same `SERVER_HOST`, `SERVER_USER`, `SERVER_SSH_KEY` if deploying to same server

### Docker Installation

On your deployment server:
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

## Deployment Options

### Option 1: GitHub Actions (Automated)

1. Push to `main` branch
2. GitHub Actions will automatically:
   - Build the application
   - Create Docker images
   - Push to GitHub Container Registry
   - Deploy to your server (if SSH configured)

### Option 2: Manual Docker Deployment

**Backend:**
```bash
cd src
docker build -t versnn-backend .
docker run -d --name versnn-backend \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="YOUR_DB_CONNECTION" \
  -e Jwt__SecretKey="YOUR_JWT_SECRET" \
  -e GEMINI_API_KEY="YOUR_API_KEY" \
  --restart unless-stopped \
  versnn-backend
```

**Frontend:**
```bash
cd src/GeminiRAG.Web
docker build -t versnn-frontend .
docker run -d --name versnn-frontend \
  -p 80:80 \
  -p 443:443 \
  --restart unless-stopped \
  versnn-frontend
```

### Option 3: Docker Compose

Create `docker-compose.yml` in project root:

```yaml
version: '3.8'

services:
  backend:
    build:
      context: ./src
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Jwt__SecretKey=${JWT_SECRET_KEY}
      - GEMINI_API_KEY=${GEMINI_API_KEY}
    restart: unless-stopped
    networks:
      - versnn-network

  frontend:
    build:
      context: ./src/GeminiRAG.Web
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - backend
    restart: unless-stopped
    networks:
      - versnn-network

networks:
  versnn-network:
    driver: bridge
```

Deploy with:
```bash
docker-compose up -d
```

## SSL/HTTPS Setup

### Using Let's Encrypt (Recommended)

```bash
# Install certbot
sudo apt-get update
sudo apt-get install certbot

# Get certificate
sudo certbot certonly --standalone -d versnn.com -d www.versnn.com

# Copy certificates to frontend
sudo cp /etc/letsencrypt/live/versnn.com/fullchain.pem ./src/GeminiRAG.Web/ssl/certificate.crt
sudo cp /etc/letsencrypt/live/versnn.com/privkey.pem ./src/GeminiRAG.Web/ssl/private.key

# Rebuild frontend with SSL
cd src/GeminiRAG.Web
docker build -t versnn-frontend .
```

Then uncomment the HTTPS section in `nginx.conf`.

## Database Setup

### Production SQL Server

1. Create production database
2. Run migrations:
```bash
cd src/GeminiRAG.Api
dotnet ef database update --connection "YOUR_PRODUCTION_CONNECTION_STRING"
```

### Or use SQL Server in Docker

```yaml
# Add to docker-compose.yml
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - versnn-network

volumes:
  sqlserver-data:
```

## Environment Variables

Create `.env` file (DO NOT commit to Git):

```env
# Database
DB_CONNECTION_STRING=Server=localhost,1433;Database=VersnnDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True

# JWT
JWT_SECRET_KEY=your-super-secret-key-minimum-32-characters-long

# Google Gemini
GEMINI_API_KEY=your-gemini-api-key

# Google OAuth (optional)
GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-client-secret
```

## Monitoring & Logs

**View logs:**
```bash
# Backend
docker logs -f versnn-backend

# Frontend  
docker logs -f versnn-frontend
```

**Health checks:**
- Backend: http://versnn.com:5000/health
- Frontend: http://versnn.com/

## Rollback

```bash
# Stop and remove current containers
docker stop versnn-backend versnn-frontend
docker rm versnn-backend versnn-frontend

# Pull previous version
docker pull ghcr.io/your-repo/backend:previous-tag
docker pull ghcr.io/your-repo/frontend:previous-tag

# Run previous version
# ... (use same docker run commands with :previous-tag)
```

## Troubleshooting

**Container won't start:**
```bash
docker logs versnn-backend
docker logs versnn-frontend
```

**Database connection issues:**
- Check connection string
- Ensure SQL Server is accessible
- Verify firewall rules

**CORS errors:**
- Update CORS settings in `Program.cs`
- Set correct API URL in Angular environment

## Production Checklist

- [ ] SSL certificates configured
- [ ] Environment variables set
- [ ] Database migrations applied
- [ ] Secrets configured in GitHub
- [ ] DNS pointing to server
- [ ] Firewall rules configured
- [ ] Backups configured
- [ ] Monitoring set up
- [ ] Error tracking configured (e.g., Sentry)

## Support

For issues, check logs and review configuration files.
