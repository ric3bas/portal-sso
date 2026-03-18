# sso

Projeto WebAPI com arquitetura vertical slice.

## Estrutura

    - Usuario
        - Controller            
    
  	- Service
          - Infra
              - Domain

Domínio é obrigatório.

### Exemplo de vertical slice:
- UsuarioController
- UsuarioService
- UsuarioRepository
- Usuario (Domain)

## Banco de Dados (PostgreSQL)

O projeto usa um modelo SaaS multi-tenant com as features:
- Usuario
- Parceiro
- ParceiroUsuario
- Regra
- Permissao
- RegraPermissao
- ParceiroUsuarioRegra
- Cliente
- Sessao
- PasswordResetToken

O schema SQL esta em `database/init/001_saas_multitenant.sql`.

No `docker-compose.yml`, essa pasta e montada em `/docker-entrypoint-initdb.d`, entao o Postgres executa o script automaticamente na primeira inicializacao do volume.

Comandos:

```bash
docker compose up -d
```

Para recriar do zero (aplicando o script novamente):

```bash
docker compose down -v
docker compose up -d
```

[![Coverage](https://img.shields.io/codecov/c/gh/ric3bas/portal-sso/main)](https://codecov.io/gh/ric3bas/portal-sso)

   Para gerar cobertura local:
   1. `dotnet test tests/sso-tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/`
   2. Envie `coverage.opencover.xml` para o serviço (Codecov) via `bash <(curl -s https://codecov.io/bash)` ou uso do action.
