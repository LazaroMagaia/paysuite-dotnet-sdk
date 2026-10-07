# PaySuite .NET SDK

SDK não oficial em .NET para integração com a API da PaySuite.

Este projeto disponibiliza uma API .NET tipada e organizada por recursos para facilitar a integração com os serviços da PaySuite.

> ⚠️ **Aviso:** este é um SDK não oficial. Este projeto não é afiliado, patrocinado, mantido ou oficialmente suportado pela PaySuite.

---

## 📚 Documentação oficial

A documentação oficial da API pode ser consultada em:

https://paysuite.tech/docs

---

# 🚀 Funcionalidades

| Recurso  | Operações                                                |
| -------- | -------------------------------------------------------- |
| Payments | Criar, consultar e listar pagamentos                     |
| Payouts  | Criar, consultar e listar payouts                        |
| Refunds  | Criar, consultar e listar reembolsos                     |
| Contacts | Criar, consultar, atualizar, eliminar e listar contactos |

---

# 📦 Instalação

Instale o SDK através do NuGet:

```bash
dotnet add package PaySuite.Sdk
```

Depois:

```csharp
using PaySuite.Sdk;
```

---

# 🔑 Autenticação

O SDK utiliza um token Bearer para autenticação.

É recomendado utilizar uma variável de ambiente.

### Linux / macOS

```bash
export PAYSUITE_TOKEN="seu-token"
```

### Windows PowerShell

```powershell
$env:PAYSUITE_TOKEN="seu-token"
```

No código:

```csharp
var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    throw new InvalidOperationException(
        "PAYSUITE_TOKEN não configurado."
    );
}
```

---

# 🏁 Quick Start

```csharp
using PaySuite.Sdk;

var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    throw new InvalidOperationException(
        "PAYSUITE_TOKEN não configurado."
    );
}

using var client = new PaySuiteClient(token);
```

Depois disso:

```csharp
client.Payments
client.Payouts
client.Refunds
client.Contacts
```

ficam disponíveis para utilização.

---

# 📦 Estrutura das respostas

Os métodos dos Resources retornam `ApiResponse<T>`.

Por exemplo:

```csharp
var response = await client.Payments.GetAsync(
    "01m487646hbykkwxzvknchfp0w"
);
```

Neste caso:

```text
ApiResponse<Payment>
├── Success
├── Message
├── Status
└── Data
    └── Payment
        ├── Id
        ├── Amount
        ├── Reference
        └── Status
```

## Response vs Data

É importante distinguir os dados da resposta dos dados do recurso.

### Dados da resposta

```csharp
response.Message
response.Status
```

Estas propriedades pertencem ao `ApiResponse<T>`.

### Dados do recurso

O recurso retornado encontra-se dentro de `Data`.

```csharp
response.Data?.Id
response.Data?.Amount
response.Data?.Reference
response.Data?.Status
```

Por exemplo:

```csharp
var response = await client.Payments.GetAsync(paymentId);

Console.WriteLine($"Message : {response.Message}");
Console.WriteLine($"Status  : {response.Status}");

Console.WriteLine($"Id        : {response.Data?.Id}");
Console.WriteLine($"Amount    : {response.Data?.Amount} MZN");
Console.WriteLine($"Reference : {response.Data?.Reference}");
Console.WriteLine($"PayStatus : {response.Data?.Status}");
```

### Guardar o `Data` numa variável

Também é possível extrair o recurso:

```csharp
var response = await client.Payments.GetAsync(paymentId);

var payment = response.Data;
```

A partir desse momento:

```csharp
payment?.Id
payment?.Amount
payment?.Reference
payment?.Status
```

Neste caso já não precisamos de utilizar `Data`, porque `payment` é o objeto `Payment`.

A regra é simples:

```text
response.X
    ↓
propriedade do ApiResponse<T>

response.Data?.X
    ↓
propriedade do recurso T
```

---

# 💳 Payments

O recurso `Payments` permite criar, consultar e listar pedidos de pagamento.

```csharp
client.Payments
```

---

## Criar Payment

Endpoint:

```text
POST /api/v1/payments
```

Request:

```csharp
var request = new CreatePaymentRequest
{
    Amount = 100.50m,
    Method = "mpesa",
    Reference = "INV123456",
    Description = "Pagamento da fatura",
    ReturnUrl = "https://example.com/success",
    WebhookUrl = "https://example.com/webhooks/paysuite",
    ContactId = "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
};

var response = await client.Payments.CreateAsync(request);
```

### JSON Body

A requisição será equivalente a:

```json
{
  "amount": 100.5,
  "method": "mpesa",
  "reference": "INV123456",
  "description": "Pagamento da fatura",
  "return_url": "https://example.com/success",
  "webhook_url": "https://example.com/webhooks/paysuite",
  "contact_id": "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
}
```

### Campos

| Campo         | Tipo    | Obrigatório | Descrição                              |
| ------------- | ------- | ----------: | -------------------------------------- |
| `amount`      | decimal |         Sim | Valor em MZN. Entre 10 e 1.000.000     |
| `method`      | string  |         Não | `credit_card`, `mpesa` ou `emola`      |
| `reference`   | string  |         Sim | Referência única, máximo 50 caracteres |
| `description` | string  |         Não | Máximo 125 caracteres                  |
| `return_url`  | string  |         Não | URL para retorno após o pagamento      |
| `webhook_url` | string  |         Não | URL de webhook                         |
| `contact_id`  | string  |         Não | ULID de um contacto                    |

Se `webhook_url` não for informado, será utilizado o webhook configurado para a conta, quando aplicável.

---

## Exemplo completo

```csharp
var response = await client.Payments.CreateAsync(
    new CreatePaymentRequest
    {
        Amount = 100.50m,
        Method = "mpesa",
        Reference = "INV123456",
        Description = "Pagamento da fatura",
        ReturnUrl = "https://example.com/success",
        WebhookUrl = "https://example.com/webhooks/paysuite",
        ContactId = "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
    }
);

Console.WriteLine($"Message : {response.Message}");
Console.WriteLine($"Status  : {response.Status}");

var payment = response.Data;

Console.WriteLine($"Id        : {payment?.Id}");
Console.WriteLine($"Amount    : {payment?.Amount} MZN");
Console.WriteLine($"Reference : {payment?.Reference}");
Console.WriteLine($"Status    : {payment?.Status}");
```

---

## Consultar Payment

Endpoint:

```text
GET /api/v1/payments/{id}
```

```csharp
var response = await client.Payments.GetAsync(
    "01m487646hbykkwxzvknchfp0w"
);
```

Exemplo:

```csharp
Console.WriteLine($"Response Status : {response.Status}");

var payment = response.Data;

Console.WriteLine($"Payment Id      : {payment?.Id}");
Console.WriteLine($"Amount          : {payment?.Amount} MZN");
Console.WriteLine($"Reference       : {payment?.Reference}");
Console.WriteLine($"Payment Status  : {payment?.Status}");
```

---

## Listar Payments

Endpoint:

```text
GET /api/v1/payments
```

```csharp
var response = await client.Payments.ListAsync(
    page: 1,
    limit: 20
);
```

Os dados da paginação encontram-se em:

```csharp
var page = response.links;
```

Exemplo:

```csharp
if (page?.Items is not null)
{
    foreach (var payment in page.Items)
    {
        Console.WriteLine(
            $"{payment.Id} - {payment.Amount} MZN - {payment.Status}"
        );
    }
}
```

---

# 💸 Payouts

O recurso `Payouts` permite criar, consultar e listar payouts.

```csharp
client.Payouts
```

---

## Criar Payout

Endpoint:

```text
POST /api/v1/payouts
```

Um payout pode utilizar um método mobile ou bancário.

Métodos mobile:

```text
mpesa
emola
mkesh
```

Métodos bancários:

```text
bank
bank_transfer
```

### Payout Mobile

```csharp
var request = new CreatePayoutRequest
{
    Amount = 100m,
    Currency = "MZN",
    Method = "mpesa",
    Reference = "PO123456",
    Description = "Pagamento ao beneficiário",
    Beneficiary = new PayoutBeneficiary
    {
        Phone = "841234567",
        Holder = "John Doe"
    },
    WebhookUrl = "https://example.com/webhooks/payout"
};

var response = await client.Payouts.CreateAsync(request);
```

JSON:

```json
{
  "amount": 100,
  "currency": "MZN",
  "method": "mpesa",
  "reference": "PO123456",
  "description": "Pagamento ao beneficiário",
  "beneficiary": {
    "phone": "841234567",
    "holder": "John Doe"
  },
  "webhook_url": "https://example.com/webhooks/payout"
}
```

### Payout Bancário

Para `bank` ou `bank_transfer`, utiliza-se o NIB:

```csharp
var request = new CreatePayoutRequest
{
    Amount = 1000m,
    Currency = "MZN",
    Method = "bank",
    Reference = "PO123456",
    Description = "Transferência bancária",
    Beneficiary = new PayoutBeneficiary
    {
        Nib = "123456789012345678901",
        Holder = "John Doe"
    }
};
```

JSON:

```json
{
  "amount": 1000,
  "currency": "MZN",
  "method": "bank",
  "reference": "PO123456",
  "description": "Transferência bancária",
  "beneficiary": {
    "nib": "123456789012345678901",
    "holder": "John Doe"
  }
}
```

### Campos

| Campo                | Tipo    | Obrigatório | Descrição                                          |
| -------------------- | ------- | ----------: | -------------------------------------------------- |
| `amount`             | decimal |         Sim | 1 – 1.000.000 MZN                                  |
| `currency`           | string  |         Sim | Atualmente `MZN`                                   |
| `method`             | string  |         Sim | `mpesa`, `emola`, `mkesh`, `bank`, `bank_transfer` |
| `reference`          | string  |         Sim | Alfanumérica, máximo 30 caracteres                 |
| `description`        | string  |         Não | Máximo 255 caracteres                              |
| `beneficiary.phone`  | string  | Condicional | 9 dígitos para métodos mobile                      |
| `beneficiary.nib`    | string  | Condicional | 21 dígitos para métodos bancários                  |
| `beneficiary.holder` | string  |         Sim | Nome do titular                                    |
| `webhook_url`        | string  |         Não | URL do webhook                                     |

---

## Consultar Payout

```csharp
var response = await client.Payouts.GetAsync(
    "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
);

var payout = response.Data;

Console.WriteLine($"Id      : {payout?.Id}");
Console.WriteLine($"Amount  : {payout?.Amount} MZN");
Console.WriteLine($"Status  : {payout?.Status}");
```

---

## Listar Payouts

```csharp
var response = await client.Payouts.ListAsync(
    page: 1,
    limit: 15
);

var page = response.Data;

if (page?.Items is not null)
{
    foreach (var payout in page.Items)
    {
        Console.WriteLine(
            $"{payout.Id} - {payout.Amount} MZN - {payout.Status}"
        );
    }
}
```

---

# 🔄 Refunds

O recurso `Refunds` permite criar, consultar e listar reembolsos.

```csharp
client.Refunds
```

---

## Criar Refund

Endpoint:

```text
POST /api/v1/refunds
```

```csharp
var request = new CreateRefundRequest
{
    PaymentId = "01H8X9V8X9Y8Z9A8B8C8D8E8F8",
    Amount = 50m,
    Reason = "Customer requested refund",
    WebhookUrl = "https://example.com/webhooks/refund"
};

var response = await client.Refunds.CreateAsync(request);
```

JSON:

```json
{
  "payment_id": "01H8X9V8X9Y8Z9A8B8C8D8E8F8",
  "amount": 50,
  "reason": "Customer requested refund",
  "webhook_url": "https://example.com/webhooks/refund"
}
```

### Campos

| Campo         | Tipo    | Obrigatório | Descrição                     |
| ------------- | ------- | ----------: | ----------------------------- |
| `payment_id`  | string  |         Sim | ULID do pagamento             |
| `amount`      | decimal |         Sim | Valor do reembolso            |
| `reason`      | string  |         Sim | Motivo, máximo 500 caracteres |
| `webhook_url` | string  |         Não | URL do webhook                |

O refund pode ser total ou parcial, dependendo do valor informado e das regras da API.

---

## Consultar Refund

```csharp
var response = await client.Refunds.GetAsync(
    "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
);

var refund = response.Data;

Console.WriteLine($"Id     : {refund?.Id}");
Console.WriteLine($"Amount : {refund?.Amount} MZN");
Console.WriteLine($"Status : {refund?.Status}");
```

---

## Listar Refunds

```csharp
var response = await client.Refunds.ListAsync(
    page: 1,
    limit: 20
);

var page = response.Data;

if (page?.Items is not null)
{
    foreach (var refund in page.Items)
    {
        Console.WriteLine(
            $"{refund.Id} - {refund.Amount} MZN - {refund.Status}"
        );
    }
}
```

---

# 👤 Contacts

O recurso `Contacts` permite criar, consultar, atualizar, eliminar e listar contactos.

```csharp
client.Contacts
```

---

## Criar Contact

Endpoint:

```text
POST /api/v1/contacts
```

Um contacto deve possuir **email ou telefone**.

```csharp
var request = new CreateContactRequest
{
    Name = "João Silva",
    Email = "joao@example.com",
    Phone = "+258841234567"
};

var response = await client.Contacts.CreateAsync(request);
```

JSON:

```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "phone": "+258841234567"
}
```

Também é possível criar apenas com email:

```json
{
  "name": "João Silva",
  "email": "joao@example.com"
}
```

Ou apenas com telefone:

```json
{
  "name": "João Silva",
  "phone": "+258841234567"
}
```

### Campos

| Campo   | Tipo   | Obrigatório | Descrição                 |
| ------- | ------ | ----------: | ------------------------- |
| `name`  | string |         Sim | Nome do contacto          |
| `email` | string | Condicional | Email do contacto         |
| `phone` | string | Condicional | Telefone em formato E.164 |

> É obrigatório fornecer pelo menos `email` ou `phone`.

---

## Consultar Contact

```csharp
var response = await client.Contacts.GetAsync(
    "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
);

var contact = response.Data;

Console.WriteLine($"Id    : {contact?.Id}");
Console.WriteLine($"Name  : {contact?.Name}");
Console.WriteLine($"Email : {contact?.Email}");
Console.WriteLine($"Phone : {contact?.Phone}");
```

---

## Atualizar Contact

Endpoint:

```text
PATCH /api/v1/contacts/{id}
```

O update utiliza apenas os campos que precisam de ser alterados.

```csharp
var response = await client.Contacts.UpdateAsync(
    contactId,
    new UpdateContactRequest
    {
        Name = "João Silva Atualizado",
        Email = "novo@example.com"
    }
);
```

JSON:

```json
{
  "name": "João Silva Atualizado",
  "email": "novo@example.com"
}
```

Por exemplo, para alterar apenas o telefone:

```csharp
var response = await client.Contacts.UpdateAsync(
    contactId,
    new UpdateContactRequest
    {
        Phone = "+258841234568"
    }
);
```

JSON:

```json
{
  "phone": "+258841234568"
}
```

---

## Eliminar Contact

Endpoint:

```text
DELETE /api/v1/contacts/{id}
```

```csharp
var response = await client.Contacts.DeleteAsync(
    contactId
);

Console.WriteLine($"Message : {response.Message}");
```

Um contacto associado a pagamentos pode não poder ser eliminado.

---

## Listar Contacts

Endpoint:

```text
GET /api/v1/contacts
```

```csharp
var response = await client.Contacts.ListAsync(
    page: 1,
    limit: 20
);
```

É possível filtrar por email:

```csharp
var response = await client.Contacts.ListAsync(
    page: 1,
    limit: 20,
    email: "joao@example.com"
);
```

Os dados encontram-se em:

```csharp
var page = response.Data;
```

---

# 🌐 Webhooks

Os recursos que suportam webhook aceitam:

```json
{
  "webhook_url": "https://example.com/webhooks/paysuite"
}
```

O campo é opcional.

Quando não é informado, a API pode utilizar o webhook configurado para a conta.

Exemplo:

```csharp
var response = await client.Payments.CreateAsync(
    new CreatePaymentRequest
    {
        Amount = 100m,
        Method = "mpesa",
        Reference = "INV123",
        WebhookUrl = "https://example.com/webhooks/paysuite"
    }
);
```

---

# 🔐 Validação de Webhooks

Caso a API forneça uma assinatura para validação do webhook, a aplicação deve validar a assinatura antes de processar os dados recebidos.

Exemplo genérico utilizando HMAC SHA-256:

```csharp
using System.Security.Cryptography;
using System.Text;

public static bool VerifySignature(
    string payload,
    string signature,
    string secret)
{
    using var hmac = new HMACSHA256(
        Encoding.UTF8.GetBytes(secret)
    );

    var hash = hmac.ComputeHash(
        Encoding.UTF8.GetBytes(payload)
    );

    var expectedSignature =
        Convert.ToHexString(hash).ToLowerInvariant();

    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expectedSignature),
        Encoding.UTF8.GetBytes(signature)
    );
}
```

A implementação final deve seguir o mecanismo de assinatura definido pela documentação oficial da PaySuite.

---

# ⚠️ Tratamento de erros

O SDK realiza validações locais antes de enviar algumas requisições.

Exemplo:

```csharp
try
{
    var response = await client.Payments.CreateAsync(
        new CreatePaymentRequest
        {
            Amount = 100m,
            Method = "mpesa",
            Reference = "INV123"
        }
    );

    Console.WriteLine(response.Message);
}
catch (PaySuiteValidationException ex)
{
    Console.WriteLine("Erro de validação:");
    Console.WriteLine(ex.Message);
}
catch (PaySuiteException ex)
{
    Console.WriteLine("Erro da PaySuite:");
    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine("Erro inesperado:");
    Console.WriteLine(ex.Message);
}
```

---

# 🛡️ Validações locais

## Payments

O SDK valida, entre outras regras:

* `amount` entre `10` e `1.000.000`
* `reference` obrigatória
* `reference` máximo de 50 caracteres
* `description` máximo de 125 caracteres
* `return_url` válida quando fornecida
* `webhook_url` válida quando fornecida

Métodos suportados:

```text
credit_card
mpesa
emola
```

---

## Payouts

O SDK valida:

* `amount` entre `1` e `1.000.000`
* `currency = MZN`
* `reference` obrigatória
* `reference` alfanumérica
* `reference` máximo de 30 caracteres
* `description` máximo de 255 caracteres
* método suportado
* telefone mobile com 9 dígitos
* NIB bancário com 21 dígitos
* `holder` obrigatório
* `webhook_url` válida quando fornecida

---

## Refunds

São validados:

* `payment_id` obrigatório
* `amount` válido
* `reason` obrigatório
* `reason` máximo de 500 caracteres
* `webhook_url` válida quando fornecida

---

# 🧩 HttpClient personalizado

É possível utilizar uma instância própria de `HttpClient`.

```csharp
using var httpClient = new HttpClient();

PaySuiteClient.Configure(
    httpClient,
    new PaySuiteOptions
    {
        Token = "seu-token"
    }
);

using var client = new PaySuiteClient(httpClient);
```

Isto permite integrar o SDK com configurações próprias de `HttpClient`, handlers, proxies ou `HttpClientFactory`.

---

# ⚙️ PaySuiteOptions

Exemplo:

```csharp
var options = new PaySuiteOptions
{
    Token = token,
    BaseUrl = new Uri("https://paysuite.tech/api/v1/"),
    Timeout = TimeSpan.FromSeconds(30)
};

using var client = new PaySuiteClient(
    token,
    options
);
```

---

# 🧪 Teste rápido

Para testar o SDK rapidamente através de uma aplicação Console:

```bash
dotnet new console -n PaySuite.Test
cd PaySuite.Test
```

Adicionar referência ao SDK local:

```bash
dotnet add reference \
    ../PaySuite.Sdk/src/PaySuite.Sdk/PaySuite.Sdk.csproj
```

Configurar o token:

```bash
export PAYSUITE_TOKEN="seu-token"
```

No `Program.cs`:

```csharp
using PaySuite.Sdk;

var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("PAYSUITE_TOKEN não configurado.");
    return;
}

using var client = new PaySuiteClient(token);

var fetchedPayment =
    await client.Payments.GetAsync(
        "01m487646hbykkwxzvknchfp0w"
    );

Console.WriteLine();
Console.WriteLine("Pagamento consultado:");

Console.WriteLine(
    $"  Success   : {fetchedPayment.Success}"
);

Console.WriteLine(
    $"  Message   : {fetchedPayment.Message}"
);

Console.WriteLine(
    $"  Status    : {fetchedPayment.Status}"
);

Console.WriteLine(
    $"  Id        : {fetchedPayment.Data?.Id}"
);

Console.WriteLine(
    $"  Amount    : {fetchedPayment.Data?.Amount} MZN"
);

Console.WriteLine(
    $"  Reference : {fetchedPayment.Data?.Reference}"
);

Console.WriteLine(
    $"  PayStatus : {fetchedPayment.Data?.Status}"
);
```

Executar:

```bash
dotnet run
```

---

# 🔧 Desenvolvimento local

Durante o desenvolvimento, é possível referenciar diretamente o projeto do SDK:

```bash
dotnet add reference \
    ../PaySuite.Sdk/src/PaySuite.Sdk/PaySuite.Sdk.csproj
```

Isto permite testar alterações imediatamente sem precisar criar e instalar um novo pacote NuGet.

---

# 📁 Estrutura do projeto

```text
PaySuite.Sdk/
├── compose.yaml
├── Dockerfile
├── .dockerignore
├── .gitignore
├── PaySuite.Sdk.slnx
├── README.md
│
├── src/
│   └── PaySuite.Sdk/
│       ├── PaySuite.Sdk.csproj
│       │
│       ├── Internal/
│       │   ├── ApiClient.cs
│       │   └── Guard.cs
│       │
│       ├── Models/
│       │   ├── Payment.cs
│       │   ├── Payout.cs
│       │   ├── Refund.cs
│       │   ├── Contact.cs
│       │   └── ...
│       │
│       ├── Resources/
│       │   ├── PaymentsResource.cs
│       │   ├── PayoutsResource.cs
│       │   ├── RefundsResource.cs
│       │   └── ContactsResource.cs
│       │
│       ├── PaySuiteClient.cs
│       ├── PaySuiteOptions.cs
│       └── ...
│
└── tests/
    └── PaySuite.Sdk.Tests/
        └── PaySuite.Sdk.Tests.csproj
```

---

# 🏗️ Arquitetura

A arquitetura do SDK segue aproximadamente:

```text
Aplicação
    │
    ▼
PaySuiteClient
    │
    ├── PaymentsResource
    ├── PayoutsResource
    ├── RefundsResource
    └── ContactsResource
            │
            ▼
        ApiClient
            │
            ▼
        HttpClient
            │
            ▼
       PaySuite API
```

O `PaySuiteClient` é o ponto de entrada principal:

```csharp
using var client = new PaySuiteClient(token);
```

Os Resources ficam disponíveis através do cliente:

```csharp
client.Payments
client.Payouts
client.Refunds
client.Contacts
```

---

# 📡 ApiClient

O `ApiClient` é responsável pela comunicação HTTP com a API.

Os Resources não precisam trabalhar diretamente com `HttpClient`.

Por exemplo:

```csharp
return _api.GetDataAsync<Payment>(
    HttpMethod.Get,
    $"payments/{id}",
    ct: ct
);
```

O resultado é:

```csharp
ApiResponse<Payment>
```

Isso mantém separadas as responsabilidades:

```text
Resource
   ↓
ApiClient
   ↓
HttpClient
   ↓
API
```

---

# 📦 Resources

Os Resources fornecem uma API orientada ao domínio.

Em vez de:

```csharp
httpClient.GetAsync(
    $"payments/{id}"
);
```

o consumidor utiliza:

```csharp
await client.Payments.GetAsync(id);
```

Da mesma forma:

```csharp
await client.Payments.CreateAsync(request);

await client.Payouts.CreateAsync(request);

await client.Refunds.CreateAsync(request);

await client.Contacts.CreateAsync(request);
```

---

# 🧪 Testes

Executar os testes:

```bash
dotnet test
```

Com mais detalhes:

```bash
dotnet test --verbosity normal
```

---

# 📦 Build

Build normal:

```bash
dotnet build
```

Build de Release:

```bash
dotnet build -c Release
```

---

# 📦 Criar pacote NuGet

Limpar:

```bash
dotnet clean
```

Criar o pacote:

```bash
dotnet pack -c Release
```

O pacote será criado em:

```text
bin/Release/
```

---

# 🚀 Publicar no NuGet

Depois de testar o pacote:

```bash
dotnet nuget push \
    bin/Release/PaySuite.Sdk.1.0.0.nupkg \
    --api-key SEU_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

Nunca coloque uma API key diretamente no código ou no repositório.

---

# 🔢 Versionamento

Atualize a versão no `PaySuite.Sdk.csproj`:

```xml
<PropertyGroup>
    <Version>1.0.1</Version>
</PropertyGroup>
```

Depois:

```bash
dotnet clean
dotnet pack -c Release
```

---

# 🔒 Segurança

Nunca coloque tokens diretamente no código:

```csharp
// ❌ Evitar
var token = "seu-token";
```

Prefira:

```csharp
var token = Environment.GetEnvironmentVariable(
    "PAYSUITE_TOKEN"
);
```

Nunca faça commit de:

* tokens;
* API keys;
* credenciais;
* secrets;
* tokens de produção.

---

# 🗺️ Roadmap

* [ ] Melhor cobertura de testes
* [ ] Testes de integração
* [ ] Suporte completo a `HttpClientFactory`
* [ ] Melhor tratamento de erros HTTP
* [ ] Documentação XML / IntelliSense
* [ ] Exemplos ASP.NET Core
* [ ] Exemplos de integração
* [ ] Melhor cobertura de webhooks
* [ ] CI/CD
* [ ] Publicação automatizada no NuGet
* [ ] Suporte a novas funcionalidades da API PaySuite

---

# 🤝 Contribuição

Clone o projeto:

```bash
git clone https://github.com/LazaroMagaia/paysuite-dotnet-sdk.git
```

Entre no diretório:

```bash
cd paysuite-dotnet-sdk
```

Restaure as dependências:

```bash
dotnet restore
```

Execute os testes:

```bash
dotnet test
```

Crie uma branch:

```bash
git checkout -b feature/minha-feature
```

Depois:

```bash
git add .
git commit -m "feat: adiciona nova funcionalidade"
git push origin feature/minha-feature
```

---

# 📄 Licença

Consulte o ficheiro `LICENSE` para obter os termos completos da licença.

---

# ⚠️ Disclaimer

Este projeto é um SDK desenvolvido pela comunidade e não possui vínculo oficial com a PaySuite.

A PaySuite e os seus respetivos produtos, serviços, serviços financeiros e marcas pertencem aos seus respetivos proprietários.

Para informações oficiais sobre endpoints, autenticação, parâmetros, webhooks, limites e comportamento da API, consulte:

https://paysuite.tech/docs

---

# 👨‍💻 Autor

Desenvolvido por **Lázaro Magaia**.

GitHub:

https://github.com/LazaroMagaia/paysuite-dotnet-sdk
