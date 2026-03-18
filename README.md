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

# portal-sso

[![Build and Coverage](https://github.com/ric3bas/portal-sso/actions/workflows/coverage.yml/badge.svg)](https://github.com/ric3bas/portal-sso/actions/workflows/coverage.yml)
[![Coverage](https://img.shields.io/codecov/c/github/ric3bas/portal-sso/master?label=coverage)](https://app.codecov.io/gh/ric3bas/portal-sso)

## Relatório de cobertura no GitHub

Este repositório publica cobertura de testes automaticamente via **GitHub Actions** + **Codecov**.

### Como funciona

- O workflow está em `.github/workflows/coverage.yml`
- A cada `push`/`pull request` na branch `master`:
  - restaura os pacotes
  - executa os testes do projeto `tests/sso-tests.csproj`
  - gera o arquivo `coverage.cobertura.xml`
  - envia a cobertura para o Codecov

### Configuração no GitHub

1. Acesse [Codecov](https://app.codecov.io/) e conecte o repositório `ric3bas/portal-sso`.
2. Em **GitHub > Settings > Secrets and variables > Actions**, crie o secret:
   - Nome: `CODECOV_TOKEN`
   - Valor: token do projeto no Codecov

> Para repositórios públicos, o token pode não ser obrigatório, mas manter o secret evita falhas de upload.

### Execução local (opcional)

```powershell
dotnet test tests/sso-tests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

O relatório será gerado em:

- `TestResults/<guid>/coverage.cobertura.xml`
