---- DROP SCHEMA sso;


--CREATE SCHEMA sso AUTHORIZATION "admin";

---- DROP SEQUENCE sso.escopo_id_seq;

--CREATE SEQUENCE sso.escopo_id_seq
--	INCREMENT BY 1
--	MINVALUE 1
--	MAXVALUE 2147483647
--	START 1
--	CACHE 1
--	NO CYCLE;

---- Permissions

--ALTER SEQUENCE sso.escopo_id_seq OWNER TO "admin";
--GRANT ALL ON SEQUENCE sso.escopo_id_seq TO "admin";

---- DROP SEQUENCE sso.perfil_id_seq;

--CREATE SEQUENCE sso.perfil_id_seq
--	INCREMENT BY 1
--	MINVALUE 1
--	MAXVALUE 2147483647
--	START 1
--	CACHE 1
--	NO CYCLE;

---- Permissions

--ALTER SEQUENCE sso.perfil_id_seq OWNER TO "admin";
--GRANT ALL ON SEQUENCE sso.perfil_id_seq TO "admin";

---- DROP SEQUENCE sso.recuperacao_senha_id_seq;

--CREATE SEQUENCE sso.recuperacao_senha_id_seq
--	INCREMENT BY 1
--	MINVALUE 1
--	MAXVALUE 2147483647
--	START 1
--	CACHE 1
--	NO CYCLE;

---- Permissions

--ALTER SEQUENCE sso.recuperacao_senha_id_seq OWNER TO "admin";
--GRANT ALL ON SEQUENCE sso.recuperacao_senha_id_seq TO "admin";

---- DROP SEQUENCE sso.token_atualizacao_id_seq;

--CREATE SEQUENCE sso.token_atualizacao_id_seq
--	INCREMENT BY 1
--	MINVALUE 1
--	MAXVALUE 2147483647
--	START 1
--	CACHE 1
--	NO CYCLE;

---- Permissions

--ALTER SEQUENCE sso.token_atualizacao_id_seq OWNER TO "admin";
--GRANT ALL ON SEQUENCE sso.token_atualizacao_id_seq TO "admin";

---- DROP SEQUENCE sso.usuario_id_seq;

--CREATE SEQUENCE sso.usuario_id_seq
--	INCREMENT BY 1
--	MINVALUE 1
--	MAXVALUE 2147483647
--	START 1
--	CACHE 1
--	NO CYCLE;

---- Permissions

--ALTER SEQUENCE sso.usuario_id_seq OWNER TO "admin";
--GRANT ALL ON SEQUENCE sso.usuario_id_seq TO "admin";
---- sso.escopo definição

---- Drop table

---- DROP TABLE sso.escopo;

--CREATE TABLE sso.escopo ( id serial4 NOT NULL, nome varchar(80) NOT NULL, CONSTRAINT escopo_nome_key UNIQUE (nome), CONSTRAINT escopo_pkey PRIMARY KEY (id));

---- Permissions

--ALTER TABLE sso.escopo OWNER TO "admin";
--GRANT ALL ON TABLE sso.escopo TO "admin";


---- sso.parceiro definição

---- Drop table

---- DROP TABLE sso.parceiro;

--CREATE TABLE sso.parceiro ( id uuid NOT NULL, nome varchar(100) NULL, descricao varchar(255) NULL, ativo bool DEFAULT true NULL, CONSTRAINT parceiro_pkey PRIMARY KEY (id));

---- Permissions

--ALTER TABLE sso.parceiro OWNER TO "admin";
--GRANT ALL ON TABLE sso.parceiro TO "admin";


---- sso.perfil definição

---- Drop table

---- DROP TABLE sso.perfil;

--CREATE TABLE sso.perfil ( id serial4 NOT NULL, nome varchar(50) NOT NULL, CONSTRAINT perfil_pkey PRIMARY KEY (id));

---- Permissions

--ALTER TABLE sso.perfil OWNER TO "admin";
--GRANT ALL ON TABLE sso.perfil TO "admin";


---- sso.perfil_escopo definição

---- Drop table

---- DROP TABLE sso.perfil_escopo;

--CREATE TABLE sso.perfil_escopo ( perfil_id int4 NOT NULL, escopo_id int4 NOT NULL, CONSTRAINT perfil_escopo_pkey PRIMARY KEY (perfil_id, escopo_id), CONSTRAINT fk_perfil_escopo_escopo FOREIGN KEY (escopo_id) REFERENCES sso.escopo(id) ON DELETE CASCADE, CONSTRAINT fk_perfil_escopo_perfil FOREIGN KEY (perfil_id) REFERENCES sso.perfil(id) ON DELETE CASCADE);

---- Permissions

--ALTER TABLE sso.perfil_escopo OWNER TO "admin";
--GRANT ALL ON TABLE sso.perfil_escopo TO "admin";


---- sso.usuario definição

---- Drop table

---- DROP TABLE sso.usuario;

--CREATE TABLE sso.usuario ( id serial4 NOT NULL, login varchar(50) NOT NULL, senha varchar(100) NOT NULL, parceiro_id uuid NOT NULL, perfil_id int4 NULL, email varchar(50) NOT NULL, nome varchar(100) NOT NULL, CONSTRAINT usuario_nome_key UNIQUE (login), CONSTRAINT usuario_pkey PRIMARY KEY (id), CONSTRAINT fk_usuario_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id), CONSTRAINT fk_usuario_perfil FOREIGN KEY (perfil_id) REFERENCES sso.perfil(id));

---- Permissions

--ALTER TABLE sso.usuario OWNER TO "admin";
--GRANT ALL ON TABLE sso.usuario TO "admin";


---- sso.recuperacao_senha definição

---- Drop table

---- DROP TABLE sso.recuperacao_senha;

--CREATE TABLE sso.recuperacao_senha ( id serial4 NOT NULL, usuario_id int4 NOT NULL, "token" varchar(255) NOT NULL, expira_em timestamp NOT NULL, usado bool DEFAULT false NOT NULL, CONSTRAINT recuperacao_senha_pkey PRIMARY KEY (id), CONSTRAINT recuperacao_senha_usuario_id_fkey FOREIGN KEY (usuario_id) REFERENCES sso.usuario(id));

---- Permissions

--ALTER TABLE sso.recuperacao_senha OWNER TO "admin";
--GRANT ALL ON TABLE sso.recuperacao_senha TO "admin";


---- sso.token_atualizacao definição

---- Drop table

---- DROP TABLE sso.token_atualizacao;

--CREATE TABLE sso.token_atualizacao ( id serial4 NOT NULL, "token" varchar(512) NOT NULL, expira_em timestamp NOT NULL, revogado bool DEFAULT false NOT NULL, usuario_id int4 NOT NULL, CONSTRAINT token_atualizacao_pkey PRIMARY KEY (id), CONSTRAINT token_atualizacao_token_key UNIQUE (token), CONSTRAINT fk_token_atualizacao_usuario FOREIGN KEY (usuario_id) REFERENCES sso.usuario(id) ON DELETE CASCADE);

---- Permissions

--ALTER TABLE sso.token_atualizacao OWNER TO "admin";
--GRANT ALL ON TABLE sso.token_atualizacao TO "admin";




---- Permissions

--GRANT ALL ON SCHEMA sso TO "admin";