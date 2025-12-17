# Stage 1: Build Angular app
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --prefer-offline --no-audit

# Copy source code
COPY . .

# Build the Angular app for production
RUN npm run build -- --configuration production --output-path=dist

# Stage 2: Serve with Nginx
FROM nginx:alpine AS runtime

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy SSL certificates (if you have them)
# COPY ssl/certificate.crt /etc/nginx/ssl/
# COPY ssl/private.key /etc/nginx/ssl/

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
