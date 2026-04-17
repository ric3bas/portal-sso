-- ===================================
-- SCRIPT PARA ADICIONAR PARCEIRO_ID NAS TABELAS L6A (CORRIGIDO)
-- ===================================

-- Remover constraints que falharam
ALTER TABLE l6a.categoria DROP CONSTRAINT IF EXISTS fk_categoria_parceiro;
ALTER TABLE l6a.equipamento DROP CONSTRAINT IF EXISTS fk_equipamento_parceiro;
ALTER TABLE l6a.cliente DROP CONSTRAINT IF EXISTS fk_cliente_parceiro;
ALTER TABLE l6a.locacao DROP CONSTRAINT IF EXISTS fk_locacao_parceiro;
ALTER TABLE l6a.financeiro DROP CONSTRAINT IF EXISTS fk_financeiro_parceiro;

-- Atualizar o valor padrão para o parceiro SSO
UPDATE l6a.categoria SET parceiro_id = '00000000-0000-0000-0000-000000000001';
UPDATE l6a.equipamento SET parceiro_id = '00000000-0000-0000-0000-000000000001';
UPDATE l6a.cliente SET parceiro_id = '00000000-0000-0000-0000-000000000001';
UPDATE l6a.locacao SET parceiro_id = '00000000-0000-0000-0000-000000000001';
UPDATE l6a.financeiro SET parceiro_id = '00000000-0000-0000-0000-000000000001';

-- Adicionar foreign keys para sso.parceiro
ALTER TABLE l6a.categoria ADD CONSTRAINT fk_categoria_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.equipamento ADD CONSTRAINT fk_equipamento_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.cliente ADD CONSTRAINT fk_cliente_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.locacao ADD CONSTRAINT fk_locacao_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);
ALTER TABLE l6a.financeiro ADD CONSTRAINT fk_financeiro_parceiro FOREIGN KEY (parceiro_id) REFERENCES sso.parceiro(id);