### Docker

- docker build -t nuages.identity.ui .
- docker run -it --rm -p 8002:80 --env-file ./env.list --name nuage-identity nuages.identity.ui
