namespace Infra;

internal static class DatabaseScripts
{
    public const string CreateSchema = @"
CREATE SCHEMA IF NOT EXISTS sso;

CREATE TABLE IF NOT EXISTS sso.parceiro (
    id uuid PRIMARY KEY,
    nome text NOT NULL,
    descricao text NULL,
    ativo boolean NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS sso.perfil (
    id SERIAL PRIMARY KEY,
    nome text NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS sso.escopo (
    id SERIAL PRIMARY KEY,
    nome text NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS sso.perfil_escopo (
    perfil_id int NOT NULL REFERENCES sso.perfil(id) ON DELETE CASCADE,
    escopo_id int NOT NULL REFERENCES sso.escopo(id) ON DELETE CASCADE,
    CONSTRAINT pk_perfil_escopo PRIMARY KEY (perfil_id, escopo_id)
);

CREATE TABLE IF NOT EXISTS sso.usuario (
    id SERIAL PRIMARY KEY,
    nome text NOT NULL,
    email text NOT NULL,
    login text NOT NULL UNIQUE,
    senha text NOT NULL,
    parceiro_id uuid NOT NULL REFERENCES sso.parceiro(id) ON DELETE CASCADE,
    perfil_id int NULL REFERENCES sso.perfil(id)
);

CREATE TABLE IF NOT EXISTS sso.token_atualizacao (
    id SERIAL PRIMARY KEY,
    token text NOT NULL,
    expira_em timestamptz NOT NULL,
    revogado boolean NOT NULL DEFAULT false,
    usuario_id int NOT NULL REFERENCES sso.usuario(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sso.recuperacao_senha (
    id SERIAL PRIMARY KEY,
    usuario_id int NOT NULL REFERENCES sso.usuario(id) ON DELETE CASCADE,
    token text NOT NULL,
    expira_em timestamptz NOT NULL,
    usado boolean NOT NULL DEFAULT false,
    criado_em timestamptz NOT NULL DEFAULT now()
);

";

    public const string TruncateAll = @"
TRUNCATE TABLE
    sso.token_atualizacao,
    sso.usuario,
    sso.perfil_escopo,
    sso.escopo,
    sso.perfil,
    sso.parceiro,
    sso.recuperacao_senha
RESTART IDENTITY CASCADE;
";
}
