# Cómo levantar la app

## Requisitos previos

- Docker y Docker Compose instalados
- Archivo `.env` en la raíz del proyecto (ver abajo)

## 1. Crear el archivo `.env`

Si no existe, generarlo con:

```bash
echo "Jwt__SecretKey=$(openssl rand -base64 48)" > .env
```

El `.env.example` sirve de referencia — solo necesita la `Jwt__SecretKey`.

## 2. Levantar los servicios

```bash
docker compose up -d
```

## 3. Verificar que todo esté corriendo

```bash
docker compose ps
```

Los tres servicios deben estar `Up`:

| Servicio   | Puerto local | Notas                        |
|------------|-------------|------------------------------|
| postgres   | 5432        | Debe quedar `(healthy)`      |
| backend    | 5122        | API .NET                     |
| frontend   | 4200        | Angular servido por nginx    |

## Problema conocido: frontend en `Restarting`

Nginx falla si el backend no está listo cuando el frontend arranca. Solución:

```bash
docker compose restart frontend
```

## URLs

- Frontend: http://localhost:4200
- Backend (Swagger en dev): http://localhost:5122/swagger

## Parar la app

```bash
docker compose down
```
