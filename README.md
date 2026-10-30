# PaySuite .NET SDK

Official .NET SDK for integrating with the PaySuite API.

This project provides a typed, resource-oriented .NET API that makes it easier to integrate payments, payouts, refunds, contacts and webhooks from your .NET applications.

---

## 📚 Documentation

The API documentation is available at:

https://paysuite.tech/docs

---

# 🚀 Features

| Resource | Operations                                          |
| -------- | --------------------------------------------------- |
| Payments | Create, retrieve and list payments                  |
| Payouts  | Create, retrieve and list payouts                   |
| Refunds  | Create, retrieve and list refunds                   |
| Contacts | Create, retrieve, update, delete and list contacts  |
| Webhooks | Signature validation and event parsing              |

---

# 📦 Installation

Install the SDK via NuGet:

```bash
dotnet add package PaySuite.Sdk --version 0.1.6
```

Then:

```csharp
using PaySuite.Sdk;
```

---

# 🔑 Authentication

The SDK uses a Bearer token for authentication.

Using an environment variable is recommended.

### Linux / macOS

```bash
export PAYSUITE_TOKEN="your-token"
```

### Windows PowerShell

```powershell
$env:PAYSUITE_TOKEN="your-token"
```

In code:

```csharp
var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    throw new InvalidOperationException(
        "PAYSUITE_TOKEN is not configured."
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
        "PAYSUITE_TOKEN is not configured."
    );
}

using var client = new PaySuiteClient(token);
```

The following resources are then available:

```csharp
client.Payments
client.Payouts
client.Refunds
client.Contacts
```

---

# 📦 Response structure

Resource methods return `ApiResponse<T>`.

For example:

```csharp
var response = await client.Payments.GetAsync(
    "01m487646hbykkwxzvknchfp0w"
);
```

In this case:

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

It is important to distinguish the response properties from the resource properties.

### Response properties

```csharp
response.Success
response.Message
response.Status
```

These properties belong to `ApiResponse<T>`.

### Resource properties

The returned resource is inside `Data`.

```csharp
response.Data?.Id
response.Data?.Amount
response.Data?.Reference
response.Data?.Status
```

For example:

```csharp
var response = await client.Payments.GetAsync(paymentId);

Console.WriteLine($"Message : {response.Message}");
Console.WriteLine($"Status  : {response.Status}");

Console.WriteLine($"Id        : {response.Data?.Id}");
Console.WriteLine($"Amount    : {response.Data?.Amount} MZN");
Console.WriteLine($"Reference : {response.Data?.Reference}");
Console.WriteLine($"PayStatus : {response.Data?.Status}");
```

### Storing `Data` in a variable

You can also extract the resource:

```csharp
var response = await client.Payments.GetAsync(paymentId);

var payment = response.Data;
```

From that point on:

```csharp
payment?.Id
payment?.Amount
payment?.Reference
payment?.Status
```

Here `Data` is no longer needed, because `payment` is already the `Payment` object.

The rule is simple:

```text
response.X
    ↓
property of ApiResponse<T>

response.Data?.X
    ↓
property of the resource T
```

## Checking the request status

The response `Status` represents the state of the request.

* If `Status` is **`success`**, the request was processed successfully and `Data` can be used.
* If `Status` is anything other than **`success`**, the request was not successful and `Data` **must not be used as valid response data**. Handle the error according to the information returned in `Message`.

```text
Status = success
    → Data is valid and can be used

Status ≠ success
    → Data must not be used
```

> **Important:** always check the request status before using `Data`.

---

# 💳 Payments

The `Payments` resource lets you create, retrieve and list payment requests.

```csharp
client.Payments
```

---

## Create Payment

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
    Description = "Invoice payment",
    ReturnUrl = "https://example.com/success",
    WebhookUrl = "https://example.com/webhooks/paysuite",
    ContactId = "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
};

var response = await client.Payments.CreateAsync(request);
```

### JSON Body

The request is equivalent to:

```json
{
  "amount": 100.5,
  "method": "mpesa",
  "reference": "INV123456",
  "description": "Invoice payment",
  "return_url": "https://example.com/success",
  "webhook_url": "https://example.com/webhooks/paysuite",
  "contact_id": "01H8X9V8X9Y8Z9A8B8C8D8E8F8"
}
```

### Fields

| Field         | Type    | Required | Description                                |
| ------------- | ------- | -------: | ------------------------------------------ |
| `amount`      | decimal |      Yes | Amount in MZN. Between 10 and 1,000,000    |
| `method`      | string  |       No | `credit_card`, `mpesa` or `emola`          |
| `reference`   | string  |      Yes | Unique reference, maximum 50 characters    |
| `description` | string  |       No | Maximum 125 characters                     |
| `return_url`  | string  |       No | URL to return to after the payment         |
| `webhook_url` | string  |       No | Webhook URL                                |
| `contact_id`  | string  |       No | ULID of a contact                          |

If `webhook_url` is not provided, the webhook configured for the account is used, where applicable.

### Full example

```csharp
var response = await client.Payments.CreateAsync(
    new CreatePaymentRequest
    {
        Amount = 100.50m,
        Method = "mpesa",
        Reference = "INV123456",
        Description = "Invoice payment",
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

## Retrieve Payment

Endpoint:

```text
GET /api/v1/payments/{id}
```

```csharp
var response = await client.Payments.GetAsync(
    "01m487646hbykkwxzvknchfp0w"
);

var payment = response.Data;

Console.WriteLine($"Response Status : {response.Status}");
Console.WriteLine($"Payment Id      : {payment?.Id}");
Console.WriteLine($"Amount          : {payment?.Amount} MZN");
Console.WriteLine($"Reference       : {payment?.Reference}");
Console.WriteLine($"Payment Status  : {payment?.Status}");
```

---

## List Payments

Endpoint:

```text
GET /api/v1/payments
```

```csharp
var response = await client.Payments.ListAsync(
    page: 1,
    limit: 20
);

if (response.Status == "success")
{
    var page = response.Data;

    if (page?.Items is not null)
    {
        foreach (var payment in page.Items)
        {
            Console.WriteLine(
                $"{payment.Id} - {payment.Amount} MZN - {payment.Status}"
            );
        }
    }
}
else
{
    Console.WriteLine(
        $"The request was not processed successfully: {response.Message}"
    );
}
```

---

# 💸 Payouts

The `Payouts` resource lets you create, retrieve and list payouts.

```csharp
client.Payouts
```

---

## Create Payout

Endpoint:

```text
POST /api/v1/payouts
```

A payout can use a mobile or a bank method.

Mobile methods:

```text
mpesa
emola
mkesh
```

Bank methods:

```text
bank
bank_transfer
```

### Mobile Payout

```csharp
var request = new CreatePayoutRequest
{
    Amount = 100m,
    Currency = "MZN",
    Method = "mpesa",
    Reference = "PO123456",
    Description = "Payment to beneficiary",
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
  "description": "Payment to beneficiary",
  "beneficiary": {
    "phone": "841234567",
    "holder": "John Doe"
  },
  "webhook_url": "https://example.com/webhooks/payout"
}
```

### Bank Payout

For `bank` or `bank_transfer`, the NIB is used:

```csharp
var request = new CreatePayoutRequest
{
    Amount = 1000m,
    Currency = "MZN",
    Method = "bank",
    Reference = "PO123456",
    Description = "Bank transfer",
    Beneficiary = new PayoutBeneficiary
    {
        Nib = "123456789012345678901",
        Holder = "John Doe"
    }
};

var response = await client.Payouts.CreateAsync(request);
```

JSON:

```json
{
  "amount": 1000,
  "currency": "MZN",
  "method": "bank",
  "reference": "PO123456",
  "description": "Bank transfer",
  "beneficiary": {
    "nib": "123456789012345678901",
    "holder": "John Doe"
  }
}
```

### Fields

| Field                | Type    | Required    | Description                                        |
| -------------------- | ------- | ----------: | -------------------------------------------------- |
| `amount`             | decimal |         Yes | 1 – 1,000,000 MZN                                  |
| `currency`           | string  |         Yes | Currently `MZN`                                    |
| `method`             | string  |         Yes | `mpesa`, `emola`, `mkesh`, `bank`, `bank_transfer` |
| `reference`          | string  |         Yes | Alphanumeric, maximum 30 characters                |
| `description`        | string  |          No | Maximum 255 characters                             |
| `beneficiary.phone`  | string  | Conditional | 9 digits for mobile methods                        |
| `beneficiary.nib`    | string  | Conditional | 21 digits for bank methods                         |
| `beneficiary.holder` | string  |         Yes | Account holder name                                |
| `webhook_url`        | string  |          No | Webhook URL                                        |

---

## Retrieve Payout

Endpoint:

```text
GET /api/v1/payouts/{id}
```

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

## List Payouts

Endpoint:

```text
GET /api/v1/payouts
```

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

The `Refunds` resource lets you create, retrieve and list refunds.

```csharp
client.Refunds
```

---

## Create Refund

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

### Fields

| Field         | Type    | Required | Description                    |
| ------------- | ------- | -------: | ------------------------------ |
| `payment_id`  | string  |      Yes | ULID of the payment            |
| `amount`      | decimal |      Yes | Refund amount                  |
| `reason`      | string  |      Yes | Reason, maximum 500 characters |
| `webhook_url` | string  |       No | Webhook URL                    |

The refund can be full or partial, depending on the amount provided and the API rules.

---

## Retrieve Refund

Endpoint:

```text
GET /api/v1/refunds/{id}
```

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

## List Refunds

Endpoint:

```text
GET /api/v1/refunds
```

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

The `Contacts` resource lets you create, retrieve, update, delete and list contacts.

```csharp
client.Contacts
```

---

## Create Contact

Endpoint:

```text
POST /api/v1/contacts
```

A contact must have an **email or a phone number**.

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

With only an email:

```json
{
  "name": "João Silva",
  "email": "joao@example.com"
}
```

With only a phone number:

```json
{
  "name": "João Silva",
  "phone": "+258841234567"
}
```

### Fields

| Field   | Type   | Required    | Description                  |
| ------- | ------ | ----------: | ---------------------------- |
| `name`  | string |         Yes | Contact name                 |
| `email` | string | Conditional | Contact email                |
| `phone` | string | Conditional | Phone number in E.164 format |

> You must provide at least `email` or `phone`.

---

## Retrieve Contact

Endpoint:

```text
GET /api/v1/contacts/{id}
```

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

## Update Contact

Endpoint:

```text
PATCH /api/v1/contacts/{id}
```

Only the fields that need to be changed are sent.

```csharp
var response = await client.Contacts.UpdateAsync(
    contactId,
    new UpdateContactRequest
    {
        Name = "João Silva Updated",
        Email = "new@example.com"
    }
);
```

JSON:

```json
{
  "name": "João Silva Updated",
  "email": "new@example.com"
}
```

For example, to change only the phone number:

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

## Delete Contact

Endpoint:

```text
DELETE /api/v1/contacts/{id}
```

```csharp
var response = await client.Contacts.DeleteAsync(contactId);

Console.WriteLine($"Message : {response.Message}");
```

A contact associated with payments may not be deletable.

---

## List Contacts

Endpoint:

```text
GET /api/v1/contacts
```

```csharp
var response = await client.Contacts.ListAsync(
    page: 1,
    limit: 20
);

var page = response.Data;
```

You can filter by email:

```csharp
var response = await client.Contacts.ListAsync(
    page: 1,
    limit: 20,
    email: "joao@example.com"
);
```

---

# 🌐 Webhooks

Resources that support webhooks accept an optional `webhook_url` field.

```json
{
  "webhook_url": "https://example.com/webhooks/paysuite"
}
```

When `webhook_url` is not provided, the API may use the webhook configured for the account.

### Example: configure a webhook when creating a payment

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

# 🔐 Webhook Validation

The SDK provides `PaySuiteWebhook` to simplify webhook integration. It reads the incoming request body, validates the `X-Signature` header, parses the event and returns a structured `WebhookResult`.

Your endpoint must be publicly accessible over HTTPS so PaySuite can reach it.

## 1. Configure the webhook secret

Set the `PAYSUITE_WEBHOOK_SECRET` environment variable to the secret used to validate webhook signatures.

```bash
PAYSUITE_WEBHOOK_SECRET=your_webhook_secret
```

Keep this secret private. Never expose it in client-side code or commit it to source control.

## 2. Implement the webhook endpoint

The following example uses ASP.NET Core Minimal APIs.

```csharp
using System.Text.Json;
using PaySuite.Sdk.Webhooks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var secret = Environment.GetEnvironmentVariable(
    "PAYSUITE_WEBHOOK_SECRET")
    ?? throw new InvalidOperationException(
        "PAYSUITE_WEBHOOK_SECRET is not configured.");

app.MapPost("/webhooks/paysuite", async (HttpRequest request) =>
{
    var result = await PaySuiteWebhook.ProcessAsync(
        request.Body,
        request.Headers["X-Signature"].ToString(),
        secret,
        request.HttpContext.RequestAborted);

    if (!result.IsValid)
    {
        Console.Error.WriteLine(
            $"Webhook rejected: {result.Status} - {result.Error}");

        return Results.Json(
            new
            {
                received = false,
                status = result.Status.ToString(),
                error = result.Error
            },
            statusCode: result.StatusCode);
    }

    var evt = result.Event!;
    var data = evt.Data;

    Console.WriteLine($"Event type: {result.Type}");
    Console.WriteLine(JsonSerializer.Serialize(evt));

    switch (result.Type)
    {
        case WebhookEventType.PaymentSuccess:
            // Mark the order identified by data.Reference as paid.
            Console.WriteLine($"Payment successful: {data.Reference}");
            break;

        case WebhookEventType.PaymentFailed:
            // Mark the order identified by data.Reference as failed.
            Console.WriteLine($"Payment failed: {data.Reference}");
            break;

        case WebhookEventType.PayoutSuccess:
            // Update the payout status in your application.
            Console.WriteLine("Payout successful.");
            break;

        case WebhookEventType.PayoutFailed:
            // Handle the failed payout.
            Console.WriteLine("Payout failed.");
            break;

        case WebhookEventType.RefundSuccess:
            // Update the refund status in your application.
            Console.WriteLine("Refund successful.");
            break;

        case WebhookEventType.RefundFailed:
            // Handle the failed refund.
            Console.WriteLine("Refund failed.");
            break;

        case WebhookEventType.Unknown:
            // Log or safely ignore unsupported event types.
            Console.WriteLine("Unknown webhook event.");
            break;
    }

    return Results.Ok(new
    {
        received = true,
        type = result.Type.ToString(),
        data
    });
});

app.Run();
```

**Important:** replace the example logging and comments with your application's business logic. Process events idempotently so duplicate deliveries do not apply the same operation more than once.

## 3. Understanding the validation result

`PaySuiteWebhook.ProcessAsync` returns a `WebhookResult` with the following properties:

| Property     | Description                                  |
| ------------ | -------------------------------------------- |
| `Status`     | Validation outcome.                          |
| `Type`       | Normalized event type.                       |
| `Event`      | Parsed webhook event, when available.        |
| `Error`      | Error description when validation fails.     |
| `IsValid`    | `true` when `Status` is `Valid`.             |
| `StatusCode` | Suggested HTTP response code.                |

### Validation statuses

| `WebhookStatus`    | Meaning                                         | HTTP status |
| ------------------ | ----------------------------------------------- | ----------: |
| `Valid`            | Signature and required event data are valid.    |    `200 OK` |
| `MissingSignature` | The `X-Signature` header is missing.            | `401 Unauthorized` |
| `InvalidSignature` | The signature does not match.                   | `401 Unauthorized` |
| `InvalidPayload`   | The payload cannot be parsed.                   | `400 Bad Request` |
| `MissingData`      | Required event fields are missing.              | `422 Unprocessable Entity` |

### Successful response

When the webhook is valid and processed successfully, return `200 OK`:

```json
{
  "received": true,
  "type": "PaymentSuccess",
  "data": {
    "id": "payment-id",
    "reference": "INV123"
  }
}
```

The exact fields inside `data` depend on the event payload.

### Error response

If validation fails, return the HTTP status suggested by `WebhookResult.StatusCode`. Example for an invalid signature (`401 Unauthorized`):

```json
{
  "received": false,
  "status": "InvalidSignature",
  "error": "Invalid signature."
}
```

**Response handling guidelines:**

* Return `200 OK` after a valid event has been accepted and processed.
* Return `400 Bad Request` for an invalid payload.
* Return `422 Unprocessable Entity` when required event data is missing.
* Return `401 Unauthorized` when the signature is missing or invalid.
* Do not return `200 OK` for a validation failure.
* Configure logging and monitoring for rejected events.

## 4. Supported event types

| Event type       | Description                                  |
| ---------------- | -------------------------------------------- |
| `PaymentSuccess` | A payment succeeded.                         |
| `PaymentFailed`  | A payment failed.                            |
| `PayoutSuccess`  | A payout succeeded.                          |
| `PayoutFailed`   | A payout failed.                             |
| `RefundSuccess`  | A refund succeeded.                          |
| `RefundFailed`   | A refund failed.                             |
| `Unknown`        | The event name is not recognized by the SDK. |

Use `WebhookResult.Type` to route events to the appropriate business logic. A valid signature and valid required data can produce a valid result with `Type = Unknown`; handle this case safely.

## 5. Security considerations

* Always validate incoming requests using `PaySuiteWebhook.ProcessAsync` or `PaySuiteWebhook.Process`.
* Preserve the original request body when validating signatures. Do not deserialize and reserialize the JSON before verification.
* Read the signature from the `X-Signature` header.
* Keep `PAYSUITE_WEBHOOK_SECRET` secure.
* Use HTTPS in production.
* Persist event identifiers and process events idempotently.
* Do not acknowledge an event before the required business operation has completed or has been durably accepted for processing.

If you already have the raw request body available, use the synchronous variant:

```csharp
var result = PaySuiteWebhook.Process(rawBody, signature, secret);
```

---

# ⚠️ Error handling

The SDK performs local validations before sending some requests.

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
    Console.WriteLine("Validation error:");
    Console.WriteLine(ex.Message);
}
catch (PaySuiteException ex)
{
    Console.WriteLine("PaySuite error:");
    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine("Unexpected error:");
    Console.WriteLine(ex.Message);
}
```

---

# 🛡️ Local validations

## Payments

* `amount` between `10` and `1,000,000`
* `reference` is required, maximum 50 characters
* `description` maximum 125 characters
* `return_url` is a valid URL when provided
* `webhook_url` is a valid URL when provided
* Supported methods: `credit_card`, `mpesa`, `emola`

## Payouts

* `amount` between `1` and `1,000,000`
* `currency` is `MZN`
* `reference` is required, alphanumeric, maximum 30 characters
* `description` maximum 255 characters
* Supported method
* Mobile phone number with 9 digits
* Bank NIB with 21 digits
* `holder` is required
* `webhook_url` is a valid URL when provided

## Refunds

* `payment_id` is required
* Valid `amount`
* `reason` is required, maximum 500 characters
* `webhook_url` is a valid URL when provided

---

# 🧩 Custom HttpClient

You can use your own `HttpClient` instance.

```csharp
using var httpClient = new HttpClient();

PaySuiteClient.Configure(
    httpClient,
    new PaySuiteOptions
    {
        Token = "your-token"
    }
);

using var client = new PaySuiteClient(httpClient);
```

This lets you integrate the SDK with your own `HttpClient` configuration, handlers, proxies or `HttpClientFactory`.

---

# ⚙️ PaySuiteOptions

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

# 🧪 Quick test

To quickly test the SDK with a console application:

```bash
dotnet new console -n PaySuite.Test
cd PaySuite.Test
dotnet add package PaySuite.Sdk
```

Configure the token:

```bash
export PAYSUITE_TOKEN="your-token"
```

In `Program.cs`:

```csharp
using PaySuite.Sdk;

var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("PAYSUITE_TOKEN is not configured.");
    return;
}

using var client = new PaySuiteClient(token);

var response = await client.Payments.GetAsync(
    "01m487646hbykkwxzvknchfp0w"
);

Console.WriteLine();
Console.WriteLine("Payment retrieved:");
Console.WriteLine($"  Success   : {response.Success}");
Console.WriteLine($"  Message   : {response.Message}");
Console.WriteLine($"  Status    : {response.Status}");
Console.WriteLine($"  Id        : {response.Data?.Id}");
Console.WriteLine($"  Amount    : {response.Data?.Amount} MZN");
Console.WriteLine($"  Reference : {response.Data?.Reference}");
Console.WriteLine($"  PayStatus : {response.Data?.Status}");
```

Run:

```bash
dotnet run
```

---

# 🔧 Local development

During development, you can reference the SDK project directly instead of the NuGet package:

```bash
dotnet add reference \
    ../paysuite-dotnet-sdk/src/PaySuite.Sdk/PaySuite.Sdk.csproj
```

This lets you test changes immediately without creating and installing a new NuGet package.

The repository also includes a console project in `samples/PaySuite.Sdk.Playground` for trying the SDK.

---

# 📁 Project structure

```text
paysuite-dotnet-sdk/
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
│       ├── Webhooks/
│       │   └── ...
│       │
│       ├── PaySuiteClient.cs
│       ├── PaySuiteOptions.cs
│       └── ...
│
├── samples/
│   └── PaySuite.Sdk.Playground/
│
└── tests/
    └── PaySuite.Sdk.Tests/
        └── PaySuite.Sdk.Tests.csproj
```

---

# 🏗️ Architecture

```text
Application
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

`PaySuiteClient` is the main entry point:

```csharp
using var client = new PaySuiteClient(token);
```

## ApiClient

`ApiClient` is responsible for HTTP communication with the API, so resources never work with `HttpClient` directly:

```csharp
return _api.GetDataAsync<Payment>(
    HttpMethod.Get,
    $"payments/{id}",
    ct: ct
);
```

The result is an `ApiResponse<Payment>`.

## Resources

Resources provide a domain-oriented API. Instead of:

```csharp
httpClient.GetAsync($"payments/{id}");
```

the consumer uses:

```csharp
await client.Payments.GetAsync(id);
await client.Payments.CreateAsync(request);
await client.Payouts.CreateAsync(request);
await client.Refunds.CreateAsync(request);
await client.Contacts.CreateAsync(request);
```

---

# 📦 Create the NuGet package

Clean:

```bash
dotnet clean
```

Create the package:

```bash
dotnet pack -c Release
```

The package is created in:

```text
bin/Release/
```

---

# 🔒 Security

Never put tokens directly in your code:

```csharp
// ❌ Avoid
var token = "your-token";
```

Prefer:

```csharp
var token = Environment.GetEnvironmentVariable("PAYSUITE_TOKEN");
```

---

# 🤝 Contributing

Clone the project:

```bash
git clone https://github.com/LazaroMagaia/paysuite-dotnet-sdk.git
cd paysuite-dotnet-sdk
```

Restore dependencies and run the tests:

```bash
dotnet restore
dotnet test
```

Create a branch, commit and push:

```bash
git checkout -b feature/my-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/my-feature
```

Then open a Pull Request.

---

# 📄 License

See the `LICENSE` file for the full license terms.

---

# 📖 Reference

For information about endpoints, authentication, parameters, webhooks, limits and API behavior, see the official documentation:

https://paysuite.tech/docs

---

# 👨‍💻 Author

Developed by **Hypertech, LDA**.

GitHub:

https://github.com/LazaroMagaia/paysuite-dotnet-sdk