-- Script rápido para atualizar todos os SQLs nos repositories para usar esquema l6a
-- Execute este script para fazer o replace em todos os repositories

-- Para Equipamento Repository
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Equipamento', 'FROM l6a.Equipamento')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Categoria', 'FROM l6a.Categoria')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Equipamento', 'INSERT INTO l6a.Equipamento')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'UPDATE Equipamento', 'UPDATE l6a.Equipamento')

-- Para Cliente Repository
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Cliente', 'FROM l6a.Cliente')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Telefone', 'FROM l6a.Telefone')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Endereco', 'FROM l6a.Endereco')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Cliente', 'INSERT INTO l6a.Cliente')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Telefone', 'INSERT INTO l6a.Telefone')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Endereco', 'INSERT INTO l6a.Endereco')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'UPDATE Cliente', 'UPDATE l6a.Cliente')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'DELETE FROM Telefone', 'DELETE FROM l6a.Telefone')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'DELETE FROM Endereco', 'DELETE FROM l6a.Endereco')

-- Para Locacao Repository  
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Locacao', 'FROM l6a.Locacao')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Locacao', 'INSERT INTO l6a.Locacao')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'UPDATE Locacao', 'UPDATE l6a.Locacao')

-- Para Financeiro Repository
UPDATE codigo SET sql_text = REPLACE(sql_text, 'FROM Financeiro', 'FROM l6a.Financeiro')
UPDATE codigo SET sql_text = REPLACE(sql_text, 'INSERT INTO Financeiro', 'INSERT INTO l6a.Financeiro')