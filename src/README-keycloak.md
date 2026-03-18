# Keycloak + JWT + Docker

## Como subir o ambiente

1. Execute:

```
docker-compose up -d
```

2. Acesse o Keycloak em: http://localhost:8080/
   - Usuário: admin
   - Senha: admin

3. Crie um Realm chamado `portal`.
4. Crie um Client (por exemplo, `sso-api`) com:
   - Access Type: `confidential` ou `public`
   - Valid Redirect URIs: `*`
   - Root URL: `http://localhost:5000/` (ajuste conforme necessário)
5. Crie um usuário para teste e atribua uma senha.

## Configuração JWT no .NET

- O sistema está configurado para validar tokens emitidos pelo Keycloak (`http://localhost:8080/realms/portal`).
- O Audience padrão do Keycloak é `account` (ajuste conforme o client criado).

## Variáveis importantes

- `Jwt:Authority`: URL do realm do Keycloak
- `Jwt:Issuer`: URL do realm do Keycloak
- `Jwt:Audience`: normalmente `account` ou o nome do client

## Observações
- O middleware de validação JWT agora delega a validação ao JwtBearer padrão do ASP.NET Core.
- Para ambientes de produção, ajuste `RequireHttpsMetadata` para `true` e utilize HTTPS.
