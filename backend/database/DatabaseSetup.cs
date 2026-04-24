using Npgsql;
using System;
using System.IO;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "Host=localhost;Port=5433;Database=portal;Username=admin;Password=admin;Pooling=true;";
        string scriptPath = "database/postgresql_l6a_schema.sql";

        try
        {
            string sqlScript = await File.ReadAllTextAsync(scriptPath);
            
            Console.WriteLine("ðŸ”Œ Conectando ao PostgreSQL...");
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("âœ… Conectado com sucesso!");
            Console.WriteLine("ðŸš€ Executando script de criaÃ§Ã£o do esquema l6a...");
            Console.WriteLine();

            string[] commands = sqlScript.Split(new string[] { ";\n", ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            int commandsExecuted = 0;
            
            foreach (string command in commands)
            {
                string cleanCommand = command.Trim();
                if (string.IsNullOrEmpty(cleanCommand) || cleanCommand.StartsWith("--"))
                    continue;

                try
                {
                    using var cmd = new NpgsqlCommand(cleanCommand, connection);
                    await cmd.ExecuteNonQueryAsync();
                    commandsExecuted++;
                    
                    if (cleanCommand.Contains("CREATE TABLE"))
                    {
                        string tableName = ExtractTableName(cleanCommand);
                        Console.WriteLine($"ðŸ“‹ Tabela criada: {tableName}");
                    }
                    else if (cleanCommand.Contains("CREATE SCHEMA"))
                    {
                        Console.WriteLine($"ðŸ—‚ï¸ Esquema l6a criado");
                    }
                    else if (cleanCommand.Contains("INSERT INTO"))
                    {
                        Console.WriteLine($"ðŸ“¥ Dados iniciais inseridos");
                    }
                    else if (cleanCommand.Contains("CREATE OR REPLACE FUNCTION"))
                    {
                        string functionName = ExtractFunctionName(cleanCommand);
                        Console.WriteLine($"âš™ï¸ FunÃ§Ã£o criada: {functionName}");
                    }
                    else if (cleanCommand.Contains("CREATE OR REPLACE VIEW"))
                    {
                        string viewName = ExtractViewName(cleanCommand);
                        Console.WriteLine($"ðŸ‘ï¸ View criada: {viewName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"âŒ Erro executando comando: {ex.Message}");
                    Console.WriteLine($"ðŸ“ Comando: {cleanCommand.Substring(0, Math.Min(cleanCommand.Length, 100))}...");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine($"ðŸŽ‰ Script executado com sucesso!");
            Console.WriteLine($"ðŸ“Š Total de comandos executados: {commandsExecuted}");
            
            Console.WriteLine();
            Console.WriteLine("ðŸ” Verificando estrutura criada...");
            
            string verifyQuery = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'l6a' 
                ORDER BY table_name;";
                
            using var verifyCmd = new NpgsqlCommand(verifyQuery, connection);
            using var reader = await verifyCmd.ExecuteReaderAsync();
            
            Console.WriteLine("ðŸ“‹ Tabelas criadas no esquema l6a:");
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"   âœ… {reader.GetString("table_name")}");
            }
            
            Console.WriteLine();
            Console.WriteLine("ðŸŽ¯ Sistema de LocaÃ§Ã£o de Equipamentos pronto para uso!");
            Console.WriteLine("ðŸ”— Esquema: l6a");
            Console.WriteLine("ðŸš€ API pode ser iniciada agora!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ðŸ’¥ Erro: {ex.Message}");
            Console.WriteLine($"ðŸ”§ Detalhes: {ex}");
        }
    }
    
    static string ExtractTableName(string command)
    {
        try
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].Equals("TABLE", StringComparison.OrdinalIgnoreCase))
                {
                    return parts[i + 1].Replace("l6a.", "").Replace("(", "");
                }
            }
        }
        catch { }
        return "tabela";
    }
    
    static string ExtractFunctionName(string command)
    {
        try
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].Equals("FUNCTION", StringComparison.OrdinalIgnoreCase))
                {
                    return parts[i + 1].Replace("l6a.", "").Replace("(", "");
                }
            }
        }
        catch { }
        return "funÃ§Ã£o";
    }
    
    static string ExtractViewName(string command)
    {
        try
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].Equals("VIEW", StringComparison.OrdinalIgnoreCase))
                {
                    return parts[i + 1].Replace("l6a.", "");
                }
            }
        }
        catch { }
        return "view";
    }
}
