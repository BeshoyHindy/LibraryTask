# Library API

This is my solution to the library assignment. It keeps books, borrowers and loans in PostgreSQL
and answers the four questions the brief asks about them: which books are borrowed most, who
borrows most, how fast a borrower reads, and which other books the readers of a given book took.
The HTTP API talks to a gRPC service, and the service is split into three modules.

## Requirements

- .NET 10 SDK
- Docker
- `grpcurl` and `jq` for the examples below, optional

## Run

```bash
docker compose up --build
```

This starts PostgreSQL, applies the migrations, seeds 12 books, 8 borrowers and 40 loans, and starts
both hosts. The gRPC service listens on port 5001, with a plain HTTP health endpoint on 5002, and
the HTTP API on port 5080. All responses in this file come from that seed.

The same stack also runs under Aspire, on the same ports, with a dashboard:

```bash
dotnet run --project src/Library.AppHost
```

## The four questions

| Question | Route |
| --- | --- |
| Most borrowed books, optionally within a date range | `GET /api/insights/most-borrowed-books?from=&to=&limit=` |
| Borrowers with the most loans in a date range | `GET /api/insights/top-borrowers?from=&to=&limit=` |
| A borrower's reading pace in pages per day | `GET /api/insights/borrowers/{id}/reading-pace` |
| Other books borrowed by readers of a book | `GET /api/insights/books/{id}/also-borrowed?limit=` |

Date ranges are inclusive. Reading pace counts closed loans only, and a same-day return counts as
one day. `limit` defaults to 10; anything above 100 is rejected.

The API also lists the books and lets you borrow and return them:

| Action | Route |
| --- | --- |
| List books | `GET /api/books` |
| Borrow a book | `POST /api/loans` with `bookId`, `borrowerId`, `borrowedOn` |
| Return a book | `POST /api/loans/{id}/return` with `returnedOn` |
| Look up a loan | `GET /api/loans/{id}` |

The OpenAPI page at [localhost:5080/scalar](http://localhost:5080/scalar) lists the same routes
and can call them.

## Calling the gRPC service

The service exposes reflection, so `grpcurl` can find the services without the proto files:

```bash
grpcurl -plaintext localhost:5001 list
```

```
grpc.health.v1.Health
grpc.reflection.v1.ServerReflection
grpc.reflection.v1alpha.ServerReflection
library.catalog.v1.CatalogService
library.insights.v1.InsightsService
library.lending.v1.LendingService
```

Borrowing book 5 for borrower 8 creates loan 41, the first id after the seeded loans. Dates are
`google.type.Date` messages:

```bash
grpcurl -plaintext -d '{"bookId":5,"borrowerId":8,"borrowedOn":{"year":2025,"month":7,"day":1}}' \
  localhost:5001 library.lending.v1.LendingService/BorrowBook | jq -c
```

```json
{"loan_id":41,"book_id":5,"borrower_id":8,"borrowed_on":{"year":2025,"month":7,"day":1}}
```

`grpcui -plaintext localhost:5001` gives the same thing as a web form.

## Calling the HTTP API

```bash
curl -s 'localhost:5080/api/insights/most-borrowed-books?limit=3' | jq -c '.[]'
```

```json
{"bookId":7,"title":"Meridian Nine","author":"Sofia Marchetti","loanCount":5}
{"bookId":2,"title":"Salt and Longitude","author":"Henrik Vasser","loanCount":5}
{"bookId":4,"title":"The Glassblower's Apprentice","author":"Elena Ferraro","loanCount":5}
```

```bash
curl -s localhost:5080/api/insights/borrowers/5/reading-pace | jq -c
```

```json
{"borrowerId":5,"pagesPerDay":33.89,"closedLoans":5}
```

Errors come back as `ProblemDetails`. Book 3 is already on loan in the seed data:

```bash
curl -s -X POST localhost:5080/api/loans -H 'content-type: application/json' \
  -d '{"bookId":3,"borrowerId":8,"borrowedOn":"2025-07-01"}' | jq -c '{status,detail}'
```

```json
{"status":409,"detail":"Book 3 is already on loan."}
```

An unknown book or loan returns 404 and an invalid request returns 400 with the field named.

## Tests

```bash
dotnet test                                              # everything
dotnet test --project tests/Library.UnitTests
dotnet test --project tests/Library.ArchitectureTests
dotnet test --project tests/Library.IntegrationTests
dotnet test --project tests/Library.FunctionalTests
dotnet test --project tests/Library.SystemTests
```

- Unit tests cover the warm-ups, the loan rules, the reading pace arithmetic, the validators and
  the status mappings. No database.
- Architecture tests check the dependency rules listed under Decisions against the compiled
  assemblies and the project files.
- Integration tests run the handlers and readers against a PostgreSQL container.
- Functional tests call every RPC through the gRPC host, in process.
- System tests call every HTTP route through the API host connected to the gRPC host.

The integration, functional and system tiers need Docker. Each test assembly starts one PostgreSQL
container and gives every test class its own copy of the seeded database. The CI workflow builds
with warnings as errors, fails if a `DbContext` has model changes without a migration, and runs all
the tests with coverage.

## Architecture

```
  HTTP client (curl, Scalar)
        |  JSON
  +-----v-----------+
  |   Library.Api   |  HTTP host, no domain logic
  +-----+-----------+
        |  gRPC
  +-----v---------------------------------------------+
  |   Library.Service                                 |
  |   +---------+    +---------+    +----------+      |
  |   | Catalog |    | Lending |    | Insights |      |
  |   +----+----+    +----+----+    +----------+      |
  |   schema catalog  schema lending  no tables       |
  +--------+--------------+---------------------------+
           |  EF Core     |
  +--------v--------------v-----+
  |         PostgreSQL          |  migrated and seeded by Library.DbMigrator
  +-----------------------------+
```

Modules call each other in process, and only through Contracts:

```
  Lending  --IBookReader-------------------->  Catalog.Contracts
  Insights --IBookReader-------------------->  Catalog.Contracts
  Insights --ILoanReader, IBorrowerReader--->  Lending.Contracts
```

| Project | Contents |
| --- | --- |
| `Library.Warmup` | The four warm-up methods. |
| `Library.Shared` | `Result`, `Error`, the module attribute, the validation pipeline and the gRPC status mapping. |
| `Catalog.Contracts`, `Lending.Contracts`, `Insights.Contracts` | The proto for each module and the reader interfaces other modules may call. |
| `Catalog`, `Lending`, `Insights` | The modules. Each has its handlers, entities, EF configuration and gRPC service. |
| `Library.Migrations` | EF migrations for the `catalog` and `lending` schemas. |
| `Library.DbMigrator` | Applies the migrations and seeds the database, then exits. |
| `Library.Service` | The gRPC host. It discovers the modules and registers them. |
| `Library.Api` | The HTTP host. One folder per module, each with its routes and response types. |
| `Library.AppHost` | The Aspire orchestrator. |
| `Library.TestSupport` | Fixtures shared by the test projects. |

## Decisions

- A module may reference another module's Contracts project and nothing else in it. Lending
  checks that a book exists through `IBookReader` from `Catalog.Contracts`. Insights reads
  loans and borrowers through `Lending.Contracts` and titles through `Catalog.Contracts`.
  `Library.ArchitectureTests` fails if any other reference appears.
- `Library.Api` does not reference any module or EF Core. It only references the Contracts
  projects, so it can only talk to the modules over gRPC.
- The HTTP routes live in `Library.Api`, not in the modules. A module's public surface is its
  proto, and the API is one client of it.
- Handlers return a `Result` instead of throwing. The module's gRPC service converts a failed
  result to a gRPC status, and the API converts the status to `ProblemDetails`.
- PostgreSQL rather than MongoDB, because all four questions are group-by or join queries.
  Migrations are applied by a separate migrator and never by the service at startup.
- Insights has no tables of its own. It reads through the other two modules' Contracts, because
  copying the data into its own schema would need an event pipeline for a few dozen rows.

## Why a modular monolith

Legacy systems came up in the first interview as a current challenge. This is the shape I would
propose for one, so I built the assignment in it. The module boundaries are enforced by the compiler
and by tests, so the team gets ownership and separate evolution of each module without running a
network between them. If one module later needs its own service, it already has a contract and a
schema, and moving it is far less work than cutting a service out of an unstructured codebase. The
cost is that all three modules share one process and one deployment, so they cannot be scaled or
released separately.

## Warm-ups

The four exercises are in `src/Library.Warmup`, with unit tests.

- `BookIds.IsPowerOfTwo(int id)` returns true for positive powers of two.
- `BookTitles.Reverse(string title)` reverses by code point, so surrogate pairs survive.
- `BookTitles.Replicate(string title, int count)` allocates the result once and fills it.
- `BookIds.PrintOddIds(TextWriter writer)` prints the odd ids from 1 to 99, one per line.
