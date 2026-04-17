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
            // Ler o script SQL
            string sqlScript = await File.ReadAllTextAsync(scriptPath);
            
            Console.WriteLine("🔌 Conectando ao PostgreSQL...");
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("✅ Conectado com sucesso!");
            Console.WriteLine("🚀 Executando script de criação do esquema l6a...");
            Console.WriteLine();

            // Dividir o script em comandos individuais (por GO ou ponto-e-vírgula)
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
                    
                    // Log para comandos importantes
                    if (cleanCommand.Contains("CREATE TABLE"))
                    {
                        string tableName = ExtractTableName(cleanCommand);
                        Console.WriteLine($"📋 Tabela criada: {tableName}");
                    }
                    else if (cleanCommand.Contains("CREATE SCHEMA"))
                    {
                        Console.WriteLine($"🗂️ Esquema l6a criado");
                    }
                    else if (cleanCommand.Contains("INSERT INTO"))
                    {
                        Console.WriteLine($"📥 Dados iniciais inseridos");
                    }
                    else if (cleanCommand.Contains("CREATE OR REPLACE FUNCTION"))
                    {
                        string functionName = ExtractFunctionName(cleanCommand);
                        Console.WriteLine($"⚙️ Função criada: {functionName}");
                    }
                    else if (cleanCommand.Contains("CREATE OR REPLACE VIEW"))
                    {
                        string viewName = ExtractViewName(cleanCommand);
                        Console.WriteLine($"👁️ View criada: {viewName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro executando comando: {ex.Message}");
                    Console.WriteLine($"📝 Comando: {cleanCommand.Substring(0, Math.Min(cleanCommand.Length, 100))}...");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine($"🎉 Script executado com sucesso!");
            Console.WriteLine($"📊 Total de comandos executados: {commandsExecuted}");
            
            // Verificar se as tabelas foram criadas
            Console.WriteLine();
            Console.WriteLine("🔍 Verificando estrutura criada...");
            
            string verifyQuery = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'l6a' 
                ORDER BY table_name;";
                
            using var verifyCmd = new NpgsqlCommand(verifyQuery, connection);
            using var reader = await verifyCmd.ExecuteReaderAsync();
            
            Console.WriteLine("📋 Tabelas criadas no esquema l6a:");
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"   ✅ {reader.GetString("table_name")}");
            }
            
            Console.WriteLine();
            Console.WriteLine("🎯 Sistema de Locação de Equipamentos pronto para uso!");
            Console.WriteLine("🔗 Esquema: l6a");
            Console.WriteLine("🚀 API pode ser iniciada agora!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro: {ex.Message}");
            Console.WriteLine($"🔧 Detalhes: {ex}");
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
        return "função";
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