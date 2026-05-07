FROM node:22-alpine AS build
WORKDIR /app

COPY frontend/biblioteca-app/package*.json ./
RUN npm ci

COPY frontend/biblioteca-app/ ./
RUN npm run build

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

COPY frontend/biblioteca-app/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/biblioteca-app/browser /usr/share/nginx/html

EXPOSE 80
