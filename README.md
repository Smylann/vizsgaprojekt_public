# vizsgaprojekt

## Backend (API + Swagger) on port 3070

The backend now binds to:

- `http://0.0.0.0:3070`

Swagger is available at:

- `http://<server>:3070/swagger`

## Frontend ↔ backend connection

Frontend API calls use:

- `window.__API_BASE_URL__` (if defined), otherwise
- `/api`

Recommended Nginx setup for frontend hosting:

```nginx
location /api/ {
    proxy_pass http://127.0.0.1:3070/api/;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

If you do not use `/api` proxying, set in HTML before loading JS modules:

```html
<script>window.__API_BASE_URL__ = "https://your-backend-host/api";</script>
```
