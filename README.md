# paysuite-dotnet-sdk

# PaySuite .NET SDK

SDK oficial em .NET para a API da [PaySuite](https://paysuite.tech/docs), a plataforma de pagamentos moçambicana. Permite receber pagamentos (M-Pesa, e-Mola, cartão), enviar payouts, fazer reembolsos e gerir contactos, com validação dos dados antes de o pedido sair da tua aplicação.

> Este projecto é desenvolvido e mantido pela PaySuite. [documentação oficial](https://paysuite.tech/docs).

## O que podes fazer

| Recurso | Operações |
|---|---|
| **Payments** (pagamentos) | criar, consultar, listar |
| **Payouts** (envio de dinheiro) | criar, consultar, listar |
| **Refunds** (reembolsos) | criar, consultar, listar |
| **Contacts** (clientes) | criar, consultar, actualizar, apagar, listar |

Todos os métodos são assíncronos (`async/await`) e aceitam um `CancellationToken`.

## Instalação

<!-- VERIFICAR: nome do pacote no NuGet e versão mínima do .NET -->

```bash
dotnet add package PaySuite.Sdk
```

## Primeiros passos

1. Cria uma conta em [paysuite.tech](https://paysuite.tech).
2. No painel, vai a **Settings > API Settings** gerar e copia o teu token.
3. Guarda o token fora do código (variável de ambiente, user-secrets, Key Vault). **Nunca o envies para o Git.**

<!-- VERIFICAR: nome da classe cliente e forma de construção -->

```csharp
using PaySuite.Sdk;

var client = new PaySuiteClient(
    Environment.GetEnvironmentVariable("PAYSUITE_TOKEN")!);
```

Os recursos ficam disponíveis no cliente: `client.Payments`, `client.Payouts`, `client.Refunds` e `client.Contacts`.

## Pagamentos

### Criar um pagamento

```csharp
using PaySuite.Sdk.Models;

var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
{
    Amount      = 100.50m,                              // obrigatório — valor em MZN
    Reference   = "INV2024001",                         // obrigatório — único, máx. 50 caracteres

    // Opcional — identifica/explica o pagamento.
    // Se não for informado, o pagamento será criado sem uma descrição.
    Description = "Pagamento da fatura 2024001",        // máx. 125 caracteres

    // Opcional — define o método de pagamento.
    // Se não for informado, a PaySuite poderá apresentar
    // as opções disponíveis no checkout, conforme a configuração
    // da conta/API.
    Method      = PaymentMethods.MPesa,

    // Opcional — URL para onde o cliente poderá ser redirecionado
    // depois de terminar o checkout.
    // Se não for informado, não haverá ReturnUrl personalizada
    // enviada pela nossa aplicação.
    ReturnUrl   = "https://oteusite.co.mz/obrigado",

    // Opcional — URL que a PaySuite utiliza para enviar
    // notificações (webhooks) sobre alterações do pagamento.
    // Se não for informado, a nossa aplicação não receberá
    // essas notificações através deste pagamento.
    WebhookUrl  = "https://oteusite.co.mz/webhooks/paysuite"
});

// URL do checkout hospedado pela PaySuite.
Console.WriteLine(payment.CheckoutUrl);
```

Métodos de pagamento disponíveis em `PaymentMethods`: `CreditCard`, `MPesa`, `Emola`.

Se não indicares `WebhookUrl`, a PaySuite usa o webhook configurado na tua conta.

### Consultar e listar

```csharp
var payment = await client.Payments.GetAsync("01H8X9V8X9Y8Z9A8B8C8D8E8F8");
Console.WriteLine(payment.Status);

var page = await client.Payments.ListAsync(page: 1, limit: 20);
foreach (var p in page.Data)
    Console.WriteLine($"{p.Reference}: {p.Amount} MZN ({p.Status})");
```

## Payouts (enviar dinheiro)

O saldo é reservado quando o payout é criado e fica reservado até ele ser concluído ou falhar.

### Para carteira móvel (M-Pesa, e-Mola, mKesh)

```csharp
var payout = await client.Payouts.CreateAsync(new CreatePayoutRequest
{
    Amount    = 500m,                 // 1 a 1.000.000 MZN
    Reference = "PO123ABC456",        // alfanumérica, máx. 30
    Method    = PayoutMethods.MPesa,
    Beneficiary = new Beneficiary
    {
        Phone  = "841234567",         // 9 dígitos, sem +258
        Holder = "João Silva"
    },
    Description = "Levantamento"      // opcional, máx. 255
});
```

### Para conta bancária

```csharp
var payout = await client.Payouts.CreateAsync(new CreatePayoutRequest
{
    Amount    = 2000m,
    Reference = "PO987XYZ654",
    Method    = PayoutMethods.Bank,   // ou PayoutMethods.BankTransfer
    Beneficiary = new Beneficiary
    {
        Nib    = "000000000000000000000",  // 21 dígitos
        Holder = "João Silva"
    }
});
```

Métodos em `PayoutMethods`: `MPesa`, `Emola`, `Mkesh`, `Bank`, `BankTransfer`. A moeda é sempre `MZN` (já é o valor por omissão).

**Estados de um payout:** `pending`, `completed`, `failed`, `cancelled`.

## Reembolsos

Só é possível reembolsar pagamentos já concluídos, até ao valor ainda disponível.

```csharp
var refund = await client.Refunds.CreateAsync(new CreateRefundRequest
{
    PaymentId = "01H8X9V8X9Y8Z9A8B8C8D8E8F8",
    Amount    = 50m,                      // 0,01 a 10.000.000
    Reason    = "Cliente pediu reembolso" // máx. 500
});
```

**Estados de um reembolso:** `pending`, `processing`, `completed`, `failed`, `cancelled`.

## Contactos

```csharp
var contact = await client.Contacts.CreateAsync(new CreateContactRequest
{
    Name  = "João Silva",
    Phone = "+258841234567"   // formato E.164; indica email ou telefone (pelo menos um)
});

await client.Contacts.UpdateAsync(contact.Id, new UpdateContactRequest { Email = "joao@exemplo.com" });

var contacts = await client.Contacts.ListAsync(email: "joao@exemplo.com");

await client.Contacts.DeleteAsync(contact.Id);
```

Não é possível apagar um contacto que tenha pagamentos associados. Para ligar um pagamento a um contacto, passa o `ContactId` ao criar o pagamento.

## Webhooks

A PaySuite avisa a tua aplicação quando algo muda. Os eventos são:

| Evento | Quando |
|---|---|
| `payment.success` / `payment.failed` | pagamento concluído ou falhado |
| `payout.success` / `payout.failed` | payout concluído ou falhado |
| `refund.success` / `refund.failed` | reembolso concluído ou falhado |

Cada pedido traz o header `X-Signature`, o HMAC-SHA256 (em hexadecimal) do corpo bruto, calculado com o teu *webhook secret*. **Verifica sempre a assinatura** antes de confiar no conteúdo. Exemplo em ASP.NET Core:

```csharp
app.MapPost("/webhooks/paysuite", async (HttpRequest http) =>
{
    using var reader = new StreamReader(http.Body);
    var payload = await reader.ReadToEndAsync();

    var secret = Environment.GetEnvironmentVariable("PAYSUITE_WEBHOOK_SECRET")!;
    var received = http.Headers["X-Signature"].ToString();

    using var hmac = new System.Security.Cryptography.HMACSHA256(
        System.Text.Encoding.UTF8.GetBytes(secret));
    var expected = Convert.ToHexString(
        hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

    var valid = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
        System.Text.Encoding.UTF8.GetBytes(expected),
        System.Text.Encoding.UTF8.GetBytes(received));

    if (!valid) return Results.Unauthorized();

    // processa o evento (payment.success, payout.failed, ...)
    return Results.Ok();
});
```

Boas práticas recomendadas pela PaySuite:

- Responde em menos de 5 segundos e processa o trabalho pesado depois.
- Usa o `id` do evento mais o estado como chave de idempotência, porque o mesmo evento pode chegar mais de uma vez.
- Há até 5 tentativas com espera crescente se a tua resposta falhar.

## Tratamento de erros

O SDK valida os dados antes de enviar o pedido. Se algo estiver errado, lança `PaySuiteValidationException` sem gastar uma chamada à API:

```csharp
try
{
    await client.Payouts.CreateAsync(request);
}
catch (PaySuiteValidationException ex)
{
    // erro nos teus dados (ex.: telefone sem 9 dígitos)
    Console.WriteLine(ex.Message);
}
<!-- VERIFICAR: nome(s) da(s) excepção(ões) para erros devolvidos pela API -->
catch (PaySuiteException ex)
{
    // erro devolvido pela API (token inválido, saldo insuficiente, ...)
    Console.WriteLine(ex.Message);
}
```

Códigos HTTP que a API pode devolver:

| Código | Significado |
|---|---|
| 400 | Dados inválidos |
| 401 | Token inválido |
| 402 | Pagamento falhou |
| 404 | Recurso não encontrado |
| 422 | Erro de validação (inclui "saldo insuficiente" em payouts) |
| 429 | Demasiados pedidos |

Limite da API: **100 pedidos por minuto** por conta.

## Regras validadas pelo SDK

| Campo | Regra |
|---|---|
| Payment `Amount` | 10 a 1.000.000 MZN <!-- VERIFICAR: mínimo de 10 não consta na doc --> |
| Payment `Reference` | obrigatória, máx. 50 caracteres |
| Payment `Description` | máx. 125 caracteres |
| Payout `Amount` | 1 a 1.000.000 MZN |
| Payout `Reference` | obrigatória, alfanumérica, máx. 30 caracteres |
| Payout `Description` | máx. 255 caracteres |
| Payout `Currency` | apenas `MZN` |
| Beneficiário móvel | `Phone` com 9 dígitos e `Holder` |
| Beneficiário bancário | `Nib` com 21 dígitos e `Holder` |
| Refund `Amount` | 0,01 a 10.000.000 |
| Refund `Reason` | obrigatória, máx. 500 caracteres |
| Listagens (`limit`) | 1 a 100 (payments, refunds, contacts) |
| URLs (`ReturnUrl`, `WebhookUrl`) | opcionais; se enviadas, têm de ser URLs válidas |

A API é a autoridade final: se as regras dela mudarem, o SDK pode ficar desactualizado. Se encontrares uma diferença, abre uma issue.

## Boas práticas

- Testa tudo no ambiente de testes antes de ires para produção.
- Usa uma `Reference` única por operação. É o que te protege de duplicados.
- Guarda o `Id` devolvido pela PaySuite junto do teu pedido, para poderes consultar o estado depois.
- Não dependas só do retorno do cliente (`ReturnUrl`): confirma o estado com o webhook ou com `GetAsync`.
- Mantém o token e o webhook secret fora do código e do repositório.

## Contribuir

Issues e pull requests são bem-vindos em [github.com/LazaroMagaia/paysuite-dotnet-sdk](https://github.com/LazaroMagaia/paysuite-dotnet-sdk).

## Licença

<!-- VERIFICAR: escolher a licença (ex.: MIT) e adicionar o ficheiro LICENSE -->