# API Auth .NET

Uma API simples para autenticação de usuários com JWT e .NET 7.0  

O Hash da senha é gerado com o algoritmo PBKDF2.

### JWT Configuration

As configurações do token JWT podem ser alteradas no arquivo `appsettings-sample.json` no diretório `./src/ApiAuth/`.

```json
{
    "JwtConfig": {
        "Secret": "This is my custom Secret key for authnetication",
        "Issuer": "http://localhost:5000",
        "Audience": "http://localhost:5000",
        "ExpirationType": "Minutes",
        "Expiration": 1,
        "ValidateIssuerSigningKey": true,
        "ValidateLifetime": true,
        "ValidateIssuer": true,
        "ValidateAudience": true,
        "RequireHttpsMetadata": false,
        "SaveToken": true
    },
}
```

É necessário criar um arquivo `appsettings.json` com a mesma estrutura do arquivo `appsettings-sample.json` em `./src/ApiAuth/`.


### Executar Testes

```bash
dotnet test
```


### Executar Aplicação

```bash
dotnet run --project ./src/ApiAuth/ApiAuth.csproj
```


### Envio de E-mail

Para enviar e-mails, é necessário configurar o arquivo `appsettings.json` com as informações do servidor SMTP. Documentação de configuração SMTP do Gmail:  
https://support.google.com/accounts/answer/185833?hl=en  


### Swagger

A documentação da API pode ser acessada em: 

http://localhost:5000/swagger/index.html


### Postman

No diretório `./postman/` há um arquivo `ApiAuth.postman_collection.json` que pode ser importado no Postman para testar a API. 
 
Também é necessário importar o arquivo `ApiAuth.postman_globals.json` para configurar as variáveis do Postman.