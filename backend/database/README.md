# Sistema de Locação de Equipamentos - Esquema l6a

## 📋 Estrutura Completa Criada

### ✅ Funcionalidades Implementadas

#### 1. **Categorias** 
- ✅ Criar, listar, filtrar por nome, atualizar e inativar
- ✅ Campos: id, nome, ativo
- ✅ Controller: `/api/v1/categorias`

#### 2. **Equipamentos**
- ✅ Incluir, listar por nome/marca/modelo/categoria, alterar e inativar
- ✅ Campos: id, nome, categoria, quantidade em estoque, preço diária, marca, modelo, numero de serie, ano de fabricação, descrição e observação internas
- ✅ Controller: `/api/v1/equipamentos`

#### 3. **Clientes**
- ✅ Incluir, alterar, listar por nome ou CPF, bloquear e inativar
- ✅ Campos: nome, cpf, telefone, email, endereço, observação
- ✅ Tabelas relacionadas: telefone (ddd, numero), endereço (logradouro, cidade, estado, numero, complemento)
- ✅ Controller: `/api/v1/clientes`

#### 4. **Locação**
- ✅ Incluir, alterar, cancelar
- ✅ Campos: id, ClientId, equipamentoId, status(ativa, devolvida, atrasada, cancelada), data da retirada, previsão da devolução, data da devolução real, valor da diaria, observação
- ✅ Ao devolver: faz cálculo de dias x valor do equipamento diário e guarda em tabela Financeiro (exceto quando cancelado)
- ✅ Controller: `/api/v1/locacoes`

#### 5. **Financeiro**
- ✅ Lançamentos automáticos na devolução de equipamentos
- ✅ Relatórios por período
- ✅ Controller: `/api/v1/financeiro`

## 🗄️ Estrutura do Banco de Dados

### Esquema: `l6a`

```sql
-- Tabelas criadas:
l6a.Categoria
l6a.Equipamento  
l6a.Cliente
l6a.Telefone
l6a.Endereco
l6a.Locacao
l6a.Financeiro
```

## 🚀 Como Configurar

### 1. **Configurar Connection String**
No `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortalSSO_Locacao;Trusted_Connection=true;MultipleActiveResultSets=true",
    "SSO_POSTGRES": "Server=(localdb)\\mssqllocaldb;Database=PortalSSO_Locacao;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 2. **Criar Estrutura do Banco**
Execute os scripts na seguinte ordem:

1. `database/setup_complete_l6a_schema.sql` - Cria toda a estrutura
2. `database/connection_string_config.sql` - Verifica se tudo foi criado corretamente
3. `database/test_database_structure.sql` - Testa com dados de exemplo

### 3. **Serviços Registrados**
Os serviços já estão registrados no `Program.cs`:

```csharp
// Categoria
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

// Equipamento  
builder.Services.AddScoped<IEquipamentoService, EquipamentoService>();
builder.Services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();

// Cliente
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

// Locacao
builder.Services.AddScoped<ILocacaoService, LocacaoService>();
builder.Services.AddScoped<ILocacaoRepository, LocacaoRepository>();

// Financeiro
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
```

## 📚 Endpoints Disponíveis

### Categorias (`/api/v1/categorias`)
- `GET /` - Lista todas as categorias
- `GET /filtro?nome={nome}` - Lista por filtro de nome
- `GET /id?id={id}` - Obter por ID
- `POST /` - Criar categoria
- `PATCH /id?id={id}` - Atualizar categoria
- `PATCH /{id}/inativar` - Inativar categoria

### Equipamentos (`/api/v1/equipamentos`)
- `GET /` - Lista todos os equipamentos
- `GET /filtro?nome={nome}&marca={marca}&modelo={modelo}&categoriaId={id}` - Lista por filtros
- `GET /id?id={id}` - Obter por ID
- `POST /` - Criar equipamento
- `PATCH /id?id={id}` - Atualizar equipamento  
- `PATCH /{id}/inativar` - Inativar equipamento

### Clientes (`/api/v1/clientes`)
- `GET /` - Lista todos os clientes
- `GET /filtro?nome={nome}&cpf={cpf}` - Lista por filtros
- `GET /id?id={id}` - Obter por ID
- `POST /` - Criar cliente
- `PATCH /id?id={id}` - Atualizar cliente
- `PATCH /{id}/bloquear` - Bloquear cliente
- `PATCH /{id}/desbloquear` - Desbloquear cliente
- `PATCH /{id}/inativar` - Inativar cliente

### Locações (`/api/v1/locacoes`)
- `GET /` - Lista todas as locações
- `GET /filtro?clienteId={id}&equipamentoId={id}&status={status}` - Lista por filtros
- `GET /atrasadas` - Lista locações atrasadas
- `GET /id?id={id}` - Obter por ID
- `POST /` - Criar locação
- `PATCH /id?id={id}` - Atualizar locação
- `PATCH /{id}/devolver` - Devolver locação (gera lançamento financeiro)
- `PATCH /{id}/cancelar` - Cancelar locação

### Financeiro (`/api/v1/financeiro`)
- `GET /` - Lista todos os lançamentos
- `GET /periodo?dataInicio={data}&dataFim={data}` - Lista por período

## 🔧 Validações Implementadas

- **CPF**: Validação completa do algoritmo
- **Email**: Formato válido
- **Datas**: Data de retirada não pode ser anterior ao hoje, previsão deve ser posterior à retirada
- **Valores**: Preços devem ser positivos
- **Disponibilidade**: Equipamento não pode ter locações conflitantes no mesmo período
- **Status**: Cliente bloqueado não pode fazer locações

## 📊 Features Especiais

- ✅ **Auto-atualização de status**: Locações ficam automaticamente "atrasadas" quando passam da data de previsão
- ✅ **Cálculo automático**: Valor total calculado automaticamente (dias x valor diária)
- ✅ **Controle de disponibilidade**: Impede locações conflitantes do mesmo equipamento
- ✅ **Lançamento financeiro**: Criado automaticamente na devolução (não no cancelamento)
- ✅ **Validação completa**: CPF, emails únicos, datas consistentes
- ✅ **Soft Delete**: Inativação em vez de exclusão física

## 🎯 Pronto para Usar!

O sistema está completamente funcional e pronto para produção. Todos os padrões do projeto foram seguidos:
- Controller → Service → Repository  
- Injeção de dependência
- Validação com FluentValidation
- Tratamento de erros
- Padrão Result para retornos
- Swagger documentation
- Logging