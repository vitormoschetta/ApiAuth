# API Auth .NET

Uma API simples para autenticação de usuários com JWT e .NET 7.0  

O Hash da senha é gerado com o algoritmo PBKDF2.

### JWT Key

A chave para gerar o token JWT é gerada a partir de um arquivo de configuração chamado `appsettings.json` na raiz do projeto.

```json
{
    "Jwt": {
        "Key": "aqui vai a sua chave"
    }
}
```

### Executar Testes

```bash
dotnet test
```

### Executar Aplicação

```bash
dotnet run --project ./src/ApiAuth/ApiAuth.csproj
```

