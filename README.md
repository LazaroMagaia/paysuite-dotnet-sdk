# PaySuite .NET SDK

Unofficial .NET SDK for integrating with the PaySuite API.

This project provides a typed, resource-oriented .NET API that makes it easier to integrate with PaySuite services.

> ⚠️ **Notice:** this is an unofficial SDK. This project is not affiliated with, sponsored by, maintained by, or officially supported by PaySuite.

---

## 📚 Official documentation

The official API documentation is available at:

https://paysuite.tech/docs

---

# 🚀 Features

| Resource | Operations                                          |
| -------- | --------------------------------------------------- |
| Payments | Create, retrieve and list payments                  |
| Payouts  | Create, retrieve and list payouts                   |
| Refunds  | Create, retrieve and list refunds                   |
| Contacts | Create, retrieve, update, delete and list contacts  |

---

# 📦 Installation

Install the SDK via NuGet:

```bash
dotnet add package PaySuite.Sdk --version 0.1.3
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

After that:

```csharp
client.Payments
client.Payouts
client.Refunds
client.Contacts
```

are available for use.

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

It is important to distinguish the response data from the resource data.

### Response data

```csharp
response.Message
response.Status
```

These properties belong to `ApiResponse<T>`.

### Resource data

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

In this case we no longer need to use `Data`, because `payment` is the `Payment` object.

The rule is simple:

```text
response.X
    ↓
property of ApiResponse<T>

response.Data?.X
    ↓
property of the resource T
```

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

The request will be equivalent to:

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

If `webhook_url` is not provided, the webhook configured for the account will be used, where applicable.

---

## Full example

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
```

Example:

```csharp
Console.WriteLine($"Response Status : {response.Status}");

var payment = response.Data;

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
```

### Request status

The **`status`** field, returned outside the `body`, represents the state of the request.

* If `status` is **`success`**, the request was processed successfully and the data returned in the `body` can be used.
* If `status` is anything other than **`success`**, the request was not successful and the `body` data **must not be used as valid response data**. In that case, handle the error according to the information returned by the API.

In other words:

```text
status = success
    → the body data is valid and can be used

status ≠ success
    → the body data must not be used
```

### Pagination

When the request is successful, the pagination data is located in:

```csharp
var page = response.links;
```

Example:

```csharp
if (response.status == "success")
{
    var page = response.links;

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
    Console.WriteLine("The request was not processed successfully.");
}
```

> **Important:** do not use the data returned in the `body` before checking the request `status`. The outer `status` determines whether the response should be considered successful and whether the data can be used.

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

You can also create one with only an email:

```json
{
  "name": "João Silva",
  "email": "joao@example.com"
}
```

Or with only a phone number:

```json
{
  "name": "João Silva",
  "phone": "+258841234567"
}
```

### Fields

| Field   | Type   | Required    | Description                |
| ------- | ------ | ----------: | -------------------------- |
| `name`  | string |         Yes | Contact name               |
| `email` | string | Conditional | Contact email              |
| `phone` | string | Conditional | Phone number in E.164 format |

> You must provide at least `email` or `phone`.

---

## Retrieve Contact

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

The update only uses the fields that need to be changed.

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
var response = await client.Contacts.DeleteAsync(
    contactId
);

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
```

You can filter by email:

```csharp
var response = await client.Contacts.ListAsync(
    page: 1,
    limit: 20,
    email: "joao@example.com"
);
```

The data is located in:

```csharp
var page = response.Data;
```

---

# 🌐 Webhooks

Resources that support webhooks accept:

```json
{
  "webhook_url": "https://example.com/webhooks/paysuite"
}
```

The field is optional.

When it is not provided, the API may use the webhook configured for the account.

Example:

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

If the API provides a signature for webhook validation, your application must validate the signature before processing the received data.

Generic example using HMAC SHA-256:

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

The final implementation must follow the signature mechanism defined in the official PaySuite documentation.

---

# ⚠️ Error handling

The SDK performs local validations before sending some requests.

Example:

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

The SDK validates, among other rules:

* `amount` between `10` and `1,000,000`
* `reference` is required
* `reference` maximum of 50 characters
* `description` maximum of 125 characters
* `return_url` is valid when provided
* `webhook_url` is valid when provided

Supported methods:

```text
credit_card
mpesa
emola
```

---

## Payouts

The SDK validates:

* `amount` between `1` and `1,000,000`
* `currency = MZN`
* `reference` is required
* `reference` is alphanumeric
* `reference` maximum of 30 characters
* `description` maximum of 255 characters
* supported method
* mobile phone number with 9 digits
* bank NIB with 21 digits
* `holder` is required
* `webhook_url` is valid when provided

---

## Refunds

The following are validated:

* `payment_id` is required
* valid `amount`
* `reason` is required
* `reason` maximum of 500 characters
* `webhook_url` is valid when provided

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

Example:

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

To quickly test the SDK with a Console application:

```bash
dotnet new console -n PaySuite.Test
cd PaySuite.Test
```

Add a reference to the local SDK:

```bash
dotnet add reference \
    ../PaySuite.Sdk/src/PaySuite.Sdk/PaySuite.Sdk.csproj
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

var fetchedPayment =
    await client.Payments.GetAsync(
        "01m487646hbykkwxzvknchfp0w"
    );

Console.WriteLine();
Console.WriteLine("Payment retrieved:");

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

Run:

```bash
dotnet run
```

---

# 🔧 Local development

During development, you can reference the SDK project directly:

```bash
dotnet add reference \
    ../PaySuite.Sdk/src/PaySuite.Sdk/PaySuite.Sdk.csproj
```

This lets you test changes immediately without having to create and install a new NuGet package.

---

# 📁 Project structure

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

# 🏗️ Architecture

The SDK architecture roughly follows:

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

Resources are available through the client:

```csharp
client.Payments
client.Payouts
client.Refunds
client.Contacts
```

---

# 📡 ApiClient

`ApiClient` is responsible for HTTP communication with the API.

Resources do not need to work with `HttpClient` directly.

For example:

```csharp
return _api.GetDataAsync<Payment>(
    HttpMethod.Get,
    $"payments/{id}",
    ct: ct
);
```

The result is:

```csharp
ApiResponse<Payment>
```

This keeps responsibilities separate:

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

Resources provide a domain-oriented API.

Instead of:

```csharp
httpClient.GetAsync(
    $"payments/{id}"
);
```

the consumer uses:

```csharp
await client.Payments.GetAsync(id);
```

Likewise:

```csharp
await client.Payments.CreateAsync(request);

await client.Payouts.CreateAsync(request);

await client.Refunds.CreateAsync(request);

await client.Contacts.CreateAsync(request);
```

---

# 🧪 Tests

Run the tests:

```bash
dotnet test
```

With more detail:

```bash
dotnet test --verbosity normal
```

---

# 📦 Build

Normal build:

```bash
dotnet build
```

Release build:

```bash
dotnet build -c Release
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

The package will be created in:

```text
bin/Release/
```

---

# 🚀 Publish to NuGet

After testing the package:

```bash
dotnet nuget push \
    bin/Release/PaySuite.Sdk.1.0.0.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

Never put an API key directly in your code or repository.

---

# 🔢 Versioning

Update the version in `PaySuite.Sdk.csproj`:

```xml
<PropertyGroup>
    <Version>1.0.1</Version>
</PropertyGroup>
```

Then:

```bash
dotnet clean
dotnet pack -c Release
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
var token = Environment.GetEnvironmentVariable(
    "PAYSUITE_TOKEN"
);
```

Never commit:

* tokens;
* API keys;
* credentials;
* secrets;
* production tokens.

---

# 🗺️ Roadmap

* [ ] Better test coverage
* [ ] Integration tests
* [ ] Full `HttpClientFactory` support
* [ ] Better HTTP error handling
* [ ] XML documentation / IntelliSense
* [ ] ASP.NET Core examples
* [ ] Integration examples
* [ ] Better webhook coverage
* [ ] CI/CD
* [ ] Automated NuGet publishing
* [ ] Support for new PaySuite API features

---

# 🤝 Contributing

Clone the project:

```bash
git clone https://github.com/LazaroMagaia/paysuite-dotnet-sdk.git
```

Enter the directory:

```bash
cd paysuite-dotnet-sdk
```

Restore dependencies:

```bash
dotnet restore
```

Run the tests:

```bash
dotnet test
```

Create a branch:

```bash
git checkout -b feature/my-feature
```

Then:

```bash
git add .
git commit -m "feat: add new feature"
git push origin feature/my-feature
```

---

# 📄 License

See the `LICENSE` file for the full license terms.

---

# ⚠️ Disclaimer

This project is an SDK developed by the community and has no official affiliation with PaySuite.

PaySuite and its respective products, services, financial services and trademarks belong to their respective owners.

For official information about endpoints, authentication, parameters, webhooks, limits and API behavior, see:

https://paysuite.tech/docs

---

# 👨‍💻 Author

Developed by **Lázaro Magaia**.

GitHub:

https://github.com/LazaroMagaia/paysuite-dotnet-sdk