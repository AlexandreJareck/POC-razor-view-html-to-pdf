# 📄 POC — Razor View HTML to PDF

Prova de conceito de geração de PDFs a partir de templates Razor com **PuppeteerSharp**, incluindo testes de performance com **BenchmarkDotNet** e **NBomber**.

---

## 🧱 Stack

| Tecnologia | Uso |
|---|---|
| .NET 10 | Runtime |
| ASP.NET Core | API Web |
| Razor Templating | Templates HTML |
| PuppeteerSharp | Renderização de PDF via Chromium |
| BenchmarkDotNet | Micro-benchmarks |
| NBomber | Load testing |
| OpenAPI + Scalar | Documentação da API |
| Bogus (Faker) | Geração de dados fake |

---

## 📁 Estrutura

```
POC-Razor-view-html-to-pdf/
├── POC-Razor-view-html-to-pdf/   # API principal
│   ├── Controllers/
│   │   └── InvoiceReportController.cs
│   ├── Views/
│   │   └── InvoiceReport.cshtml  # Template Razor do PDF
│   ├── Contracts/
│   │   ├── Invoice.cs
│   │   ├── Address.cs
│   │   └── LineItem.cs
│   ├── PuppeteerBrowserFactory.cs
│   ├── InvoiceFactory.cs
│   └── Program.cs
├── Benchmarks/                   # Micro-benchmarks
│   └── InvoiceBenchmark.cs
└── LoadTests/                    # Testes de carga
    └── Program.cs
```

---

## 🚀 Como rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### API

```bash
cd POC-Razor-view-html-to-pdf
dotnet run
```

Acesse a documentação em:

```
https://localhost:7261/scalar/v1
```

### Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/invoice-report` | Gera 10 invoices em PDF (padrão) |
| `GET` | `/invoice-report?count=5` | Gera N invoices em PDF |

---

## ⚙️ Arquitetura de geração de PDF

O `PuppeteerBrowserFactory` gerencia o ciclo de vida do Chromium com as seguintes estratégias:

- **Browser Singleton** — uma única instância do Chromium para toda a aplicação
- **SemaphoreSlim** — limita a 3 gerações de PDF simultâneas, evitando sobrecarga
- **Reconexão automática** — recria o browser caso ele caia
- **Timeout na fila** — rejeita com `503` se não houver vaga em 25 segundos

```csharp
// máximo 3 PDFs simultâneos
private readonly SemaphoreSlim _pageLock = new(3, 3);

// timeout na fila de espera
var acquired = await _pageLock.WaitAsync(TimeSpan.FromSeconds(25));
if (!acquired)
    throw new TimeoutException("Sem vagas disponíveis. Tente novamente.");
```

---

## 📊 Benchmarks

Roda os micro-benchmarks com BenchmarkDotNet:

```bash
cd Benchmarks
dotnet run -c Release
```

### Resultados (Intel Core 7 240H — .NET 10)

| Invoices | Tempo médio | Memória alocada |
|---|---|---|
| 1 | ~1.68 s | 1.48 MB |
| 5 | ~2.20 s | 4.98 MB |
| 10 | ~2.22 s | 9.32 MB |

> O tempo é dominado pela abertura de nova `Page` no Chromium. O volume de dados tem impacto mínimo — de 1 para 10 invoices, o delta é de apenas ~65ms.

---

## 🔥 Load Test

Requer a API rodando em um terminal separado:

```bash
# terminal 1 — sobe a API
dotnet run --project POC-Razor-view-html-to-pdf

# terminal 2 — roda o load test
cd LoadTests
dotnet run
```

O NBomber gera um relatório HTML ao final em `LoadTests/reports/`.

### Configuração padrão

```csharp
Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
```

---

## 🔍 Observações de performance

- **CSS inline** reduz latência
- **`DOMContentLoaded`** como `WaitUntil` é o ideal para HTML sem recursos externos
- **`SemaphoreSlim(3, 3)`** evita sobrecarga do Chromium com muitas pages simultâneas

---

## 📖 Documentação

| URL | Descrição |
|---|---|
| `/scalar/v1` | Scalar UI (recomendado) |
| `/swagger` | Swagger UI clássico |
| `/openapi/v1.json` | Spec OpenAPI em JSON |
