# InterviewPal Backend

ASP.NET Core Web API for **InterviewPal**, an interview-practice app. Every question is a small lesson:
a short answer to say in the interview, a deeper explanation with a code example, the common mistake,
and the follow-up question an interviewer usually asks next.

The question content is written in Persian; technical terms and code stay in English.

**Live app:** https://interview-pal-frontend-sable.vercel.app (frontend: [interviewPal-frontend](https://github.com/nasibehash/interviewPal-frontend))
· **API:** https://interviewpal-backend.onrender.com (`/health`, `/api/technologies`)

> Status: **Phase 1 (MVP)** – question bank + practice sessions. No login yet: the client keeps the
> learner's progress in the browser. See [Roadmap](#roadmap).

## What is in the box

| Technology | Covered versions (`supportedFrom` → `currentVersion`) | Questions |
|---|---|---|
| JavaScript | ES2024 → ES2026 | 50 |
| TypeScript | 5.9 → 7.0 | 50 |
| Angular | 20 → 22 | 50 |
| React | 18 → 19.3 | 50 |
| Next.js | 14 → 16 | 50 |
| .NET | 8 → 10 (C# 12–14) | 50 |

Each technology has 16 Junior, 18 Mid and 16 Senior questions across four types
(`MultipleChoice`, `CodeOutput`, `ShortAnswer`, `Conceptual`). The code-output questions and the claims
about new APIs were checked against the real runtime/compiler (Node, TypeScript 7, .NET 10, React 19.3,
Angular 22 signals) while the bank was written.

## Requirements

- .NET SDK 10 (`net10.0`)

## Run

```bash
dotnet run --project src/InterviewPal.Api
```

The API starts on `http://localhost:5161`. On startup it creates a SQLite database
(`interviewpal.db`, path configurable) and loads the question bank from `content/*.json`.
In Development the OpenAPI document is served at `/openapi/v1.json`.

### Configuration

| Key | Default | Purpose |
|---|---|---|
| `ConnectionStrings:Default` | `Data Source=interviewpal.db` | SQLite database |
| `Content:Path` | `<output>/content` | Directory containing the question JSON files |
| `HttpsRedirection:Enabled` | `true` | Redirect HTTP to HTTPS outside Development (the Docker image turns it off) |
| `Cors:AllowedOrigins` | `["http://localhost:4200"]` | Origins allowed to call the API (the Angular dev server) |

## Docker

```bash
docker build -t interviewpal-api .
docker run -p 8080:8080 -v interviewpal-data:/data interviewpal-api
# http://localhost:8080/health
```

The image is multi-stage (SDK to ASP.NET runtime), runs as a non-root user and keeps the SQLite database in the `/data`
volume. Settings are environment variables, for example `Cors__AllowedOrigins__0=https://app.example.com` or
`ConnectionStrings__Default="Data Source=/data/other.db"`.

## API

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Liveness check |
| GET | `/api/technologies` | Technologies with version window and question counts per level |
| GET | `/api/questions` | Browse questions – filters: `technology`, `level`, `tag`, `search`, `page`, `pageSize` (answers are **not** included) |
| GET | `/api/questions/{id}` | One question together with its answer, explanation, common mistake and follow-up |
| POST | `/api/questions/{id}/reports` | Report a wrong / outdated / unclear question |
| POST | `/api/practice/sessions` | Build a practice session (balanced random selection) |
| POST | `/api/practice/questions/{id}/check` | Grade one answer and return the explanation (learning / flashcard modes) |
| POST | `/api/practice/evaluate` | Grade a whole session (interview mode) and get scores per technology, level and weak tags |
| GET | `/api/lessons` | Algorithm and design-pattern lessons – filters: `kind` (`Algorithm`/`DesignPattern`), `level`, `category`, `technology` |
| GET | `/api/lessons/{id}?technology=react` | One lesson with the real-world scenario, explanation, the implementation written for the chosen technology and its exercises (without answers) |
| POST | `/api/lessons/{id}/exercises/{exerciseId}/check` | Grade one exercise of a lesson (`{ "choiceId": 1 }`) |

Errors are returned as RFC 7807 `ProblemDetails`.

### Practice flow

```jsonc
// 1. start a session
POST /api/practice/sessions
{ "technologies": ["angular", "react"], "levels": ["Mid", "Senior"], "count": 20, "mode": "Interview" }
// -> 20 questions (no answers), estimatedMinutes, timeLimitSeconds for Interview mode

// 2a. learning / flashcard mode: check each answer as you go
POST /api/practice/questions/angular-signal-basics-output/check
{ "choiceId": 42 }            // or { "knewIt": true } for short-answer / conceptual questions

// 2b. interview mode: submit everything at the end
POST /api/practice/evaluate
{ "answers": [ { "questionId": "…", "choiceId": 42 }, { "questionId": "…", "knewIt": false } ] }
// -> total / correct / percent, byTechnology, byLevel, weakTags, weakQuestionIds, results
```

`technologies` and `levels` may be empty (meaning "all"); `count` is between 5 and 100. If fewer
matching questions exist, fewer are returned.

### Lessons (algorithms and design patterns)

Besides the question bank there are **24 lessons** – 14 algorithms and 10 design patterns – each with a real-world
scenario (price filter, rate limiter, shipping routes, checkout …), an explanation, complexity / when to use / common
mistake, **one implementation per technology** and four exercises. The implementation is written in the idiom of the
chosen technology: plain algorithm code for JavaScript, TypeScript and C#, and an Angular service / React hook /
Next.js route handler or server action for the framework technologies.

```
content/lessons/<lesson-id>/
  lesson.json       texts (Persian), metadata, exercises, one entry per technology
  javascript.js  typescript.ts  angular.ts  react.tsx  nextjs.ts  dotnet.cs
```

Lessons are static content, so they are loaded into memory at startup (`LessonCatalog`) instead of the database.
`LessonValidator` checks the layout on startup and in the tests: all six technologies are present, every code file exists,
every lesson has at least two exercises with exactly one correct choice, algorithms declare their complexity. Choice
order is shuffled deterministically (seeded by the exercise id) so authors do not have to balance answer positions.

## Architecture

```
src/
  InterviewPal.Domain          entities and enums (no dependencies)
  InterviewPal.Application     contracts (DTOs), services, repository interfaces
  InterviewPal.Infrastructure  EF Core (SQLite), repositories, JSON content seeder + validator
  InterviewPal.Api             controllers, error handling, composition root
tests/
  InterviewPal.Api.Tests       integration tests (WebApplicationFactory) + content validation
content/                       the question bank, one JSON file per technology; lessons/ holds the 24 lessons
docs/                          implementation document (backend.md) and its PDF
```

Dependencies point inwards: `Api → Application ← Infrastructure`, `Application → Domain`.
Application services depend on repository interfaces only.

Notes:

- The database is created with `EnsureCreated` for the MVP. EF Core migrations arrive together with the
  user/history tables in Phase 2.
- The question bank is data, not code. The seeder upserts by question id and stores a content hash, so
  unchanged questions keep their choice ids across restarts, changed questions are updated, and questions
  removed from the files are deleted.

## Lesson content

A lesson is a folder under `content/lessons/`. Rules for new lessons:

1. **Run the code.** The JavaScript, TypeScript and C# files are self-contained programs
   (`node file.js`, `node file.ts`, `dotnet run file.cs`); type-check the Angular, React and Next.js files against the
   real framework typings. Put the expected output in comments.
2. **Use a real scenario** and keep the example small but complete; show the framework idiom, not a port of the plain code.
3. Do not let length give the answer away: the guard test fails if the correct choice is the unique longest one in more
   than 45% of all exercises (or the first choice in more than 45%).

## Question content

Files live in `content/<technology>.json`:

```jsonc
{
  "technology": { "slug": "angular", "name": "Angular", "currentVersion": "22", "supportedFrom": "20" },
  "questions": [
    {
      "id": "angular-signal-basics-output",     // stable slug, must start with "<technology>-"
      "level": "Junior",                         // Junior | Mid | Senior
      "type": "CodeOutput",                      // MultipleChoice | CodeOutput | ShortAnswer | Conceptual
      "text": "…", "codeSnippet": "…", "codeLanguage": "typescript",
      "shortAnswer": "…", "explanation": "…", "commonMistake": "…", "followUp": "…",
      "estimatedSeconds": 45,
      "minVersion": "17",                        // version the concept was introduced in
      "tags": ["signals", "computed"],
      "choices": [ { "text": "…" }, { "text": "…", "isCorrect": true } ]   // choice types only
    }
  ]
}
```

Rules enforced by `ContentValidator` (at startup) and by the content tests:

- ids are unique and prefixed with the technology slug; level and type are valid; at least one tag;
- choice types have 2–6 choices with **exactly one** correct; `ShortAnswer`/`Conceptual` have none;
- `CodeOutput` needs a `codeSnippet`.

Guidelines for new questions:

1. **Stay inside the version window** (`supportedFrom` → `currentVersion`) and verify claims against the
   official docs or by running the code.
2. **Do not let length give the answer away.** The correct option must not be systematically the longest
   (a test fails if it is the unique longest option in more than 45% of a technology's choice questions).
   Write plausible distractors of a similar length.
3. Put the detail in `explanation`; keep `shortAnswer` to what you would actually say in an interview.
4. Include `commonMistake` and `followUp` – they are what makes a question teach something.

## Tests

```bash
dotnet test
```

The suite starts the real application against an isolated SQLite file and covers the endpoints
(filters, paging, grading, evaluation, reports, validation and error responses) plus the integrity of the
real question bank in `content/`.

## Roadmap

1. **Phase 1 (this repo)** – question bank, practice sessions, evaluation, reports.
2. **Phase 2** – accounts, server-side history, timed interview mode, weak-topic report, ≥ 100 questions per technology.
3. **Phase 3** – spaced repetition, admin panel for questions, short coding questions.
4. **Phase 4** – AI interviewer: free-text/voice answers with feedback and follow-up questions.

## Documentation

The implementation document lives in [`docs/backend.md`](docs/backend.md) and is also available as a PDF:
[`docs/InterviewPal-Backend.pdf`](docs/InterviewPal-Backend.pdf). Update both together with the README when a change
affects how the API works.
