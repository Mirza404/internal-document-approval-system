# Multi-stage build for the React (Vite) frontend.
# The build emits a static bundle that nginx serves; nginx also reverse-proxies
# /api to the API container so the browser talks to a single origin (no CORS).

# ---- Build stage ----
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Browser API calls are made relative to the frontend origin and proxied by
# nginx (see nginx.conf). Override at build time if needed.
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ---- Runtime stage ----
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
