using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InterviewPal.Domain;
using InterviewPal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Api.Tests;

public class ProgressTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    /// <summary>A valid evaluation request: the real id of a choice of the multiple-choice question, and a self-assessment.</summary>
    private static async Task<object> Answers(HttpClient client, bool knewIt)
    {
        var detail = await client.GetFromJsonAsync<JsonElement>("/api/questions/sample-mcq-junior");
        var choiceId = detail.GetProperty("question").GetProperty("choices")[0].GetProperty("id").GetInt32();
        return new
        {
            answers = new object[]
            {
                new { questionId = "sample-short-senior", knewIt },
                new { questionId = "sample-mcq-junior", choiceId }
            },
            mode = "Interview"
        };
    }

    private async Task<int> CorrectChoiceOfMcq(Session session)
    {
        var detail = await session.GetJson("/api/questions/sample-mcq-junior");
        return detail.GetProperty("answer").GetProperty("correctChoiceIds")[0].GetInt32();
    }

    [Fact]
    public async Task Progress_endpoints_need_a_login()
    {
        var client = Accounts.NewClient(factory);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/me/progress")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync("/api/me/progress")).StatusCode);
    }

    [Fact]
    public async Task An_anonymous_evaluation_is_returned_but_not_stored()
    {
        var client = Accounts.NewClient(factory);
        var before = await factory.WithDbAsync(db => db.PracticeHistory.CountAsync());
        var response = await client.PostAsJsonAsync("/api/practice/evaluate", await Answers(client, true));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await factory.WithDbAsync(db => db.PracticeHistory.CountAsync()));
    }

    [Fact]
    public async Task A_logged_in_evaluation_is_stored_as_history_and_question_stats()
    {
        var session = await Accounts.Register(factory);
        var correct = await CorrectChoiceOfMcq(session);

        var evaluate = await session.Send(HttpMethod.Post, "/api/practice/evaluate", new
        {
            answers = new object[]
            {
                new { questionId = "sample-short-senior", knewIt = false },
                new { questionId = "sample-mcq-junior", choiceId = correct }
            },
            mode = "Interview"
        });
        Assert.Equal(HttpStatusCode.OK, evaluate.StatusCode);

        var progress = await session.GetJson("/api/me/progress");
        var entry = progress.GetProperty("history")[0];
        Assert.Equal("Interview", entry.GetProperty("mode").GetString());
        Assert.Equal(2, entry.GetProperty("total").GetInt32());
        Assert.Equal(1, entry.GetProperty("correct").GetInt32());
        Assert.Equal(50, entry.GetProperty("percent").GetInt32());
        Assert.Equal("sample", entry.GetProperty("byTechnology")[0].GetProperty("key").GetString());

        var stats = progress.GetProperty("questionStats").EnumerateArray().ToDictionary(s => s.GetProperty("questionId").GetString()!);
        Assert.True(stats["sample-mcq-junior"].GetProperty("lastCorrect").GetBoolean());
        Assert.False(stats["sample-short-senior"].GetProperty("lastCorrect").GetBoolean());
        Assert.Equal("Junior", stats["sample-mcq-junior"].GetProperty("level").GetString());
        Assert.False(string.IsNullOrEmpty(stats["sample-mcq-junior"].GetProperty("text").GetString()));
    }

    [Fact]
    public async Task Stats_accumulate_and_the_latest_answer_decides_lastCorrect()
    {
        var session = await Accounts.Register(factory);
        await session.Send(HttpMethod.Post, "/api/practice/evaluate", await Answers(session.Client, false));
        await session.Send(HttpMethod.Post, "/api/practice/evaluate", await Answers(session.Client, true));

        var progress = await session.GetJson("/api/me/progress");
        Assert.Equal(2, progress.GetProperty("history").GetArrayLength());
        var stat = progress.GetProperty("questionStats").EnumerateArray().Single(s => s.GetProperty("questionId").GetString() == "sample-short-senior");
        Assert.Equal(2, stat.GetProperty("seen").GetInt32());
        Assert.Equal(1, stat.GetProperty("correct").GetInt32());
        Assert.True(stat.GetProperty("lastCorrect").GetBoolean());
    }

    [Fact]
    public async Task One_users_progress_is_invisible_to_another()
    {
        var a = await Accounts.Register(factory);
        var b = await Accounts.Register(factory);
        await a.Send(HttpMethod.Post, "/api/practice/evaluate", await Answers(a.Client, true));

        Assert.Equal(1, (await a.GetJson("/api/me/progress")).GetProperty("history").GetArrayLength());
        Assert.Equal(0, (await b.GetJson("/api/me/progress")).GetProperty("history").GetArrayLength());
    }

    [Fact]
    public async Task Lesson_answers_are_stored_for_a_logged_in_user_and_the_latest_one_wins()
    {
        var session = await Accounts.Register(factory);
        var lesson = await session.GetJson("/api/lessons/sample-lesson?technology=sample");
        var choices = lesson.GetProperty("exercises")[0].GetProperty("choices").EnumerateArray().ToList();
        var right = choices.Single(c => c.GetProperty("text").GetString() == "right").GetProperty("id").GetInt32();
        var wrong = choices.First(c => c.GetProperty("text").GetString() == "wrong 1").GetProperty("id").GetInt32();

        await session.Send(HttpMethod.Post, "/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = wrong });
        var first = (await session.GetJson("/api/me/progress")).GetProperty("lessons")[0];
        Assert.Equal("sample-lesson", first.GetProperty("lessonId").GetString());
        Assert.Equal(2, first.GetProperty("total").GetInt32());
        Assert.False(first.GetProperty("answers").GetProperty("e1").GetBoolean());

        await session.Send(HttpMethod.Post, "/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = right });
        var second = (await session.GetJson("/api/me/progress")).GetProperty("lessons")[0];
        Assert.True(second.GetProperty("answers").GetProperty("e1").GetBoolean());
    }

    [Fact]
    public async Task An_anonymous_lesson_check_stores_nothing()
    {
        var before = await factory.WithDbAsync(db => db.LessonProgress.CountAsync());
        var response = await Accounts.NewClient(factory)
            .PostAsJsonAsync("/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = 0 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await factory.WithDbAsync(db => db.LessonProgress.CountAsync()));
    }

    private static object LocalProgress(string at = "2026-03-01T10:00:00.000Z") => new
    {
        history = new object[]
        {
            new { at, mode = "Learning", total = 10, correct = 7, byTechnology = new[] { new { key = "sample", total = 10, correct = 7, percent = 70 } } },
            new { at = "2026-03-02T10:00:00.000Z", mode = "Interview", total = 5, correct = 9, byTechnology = Array.Empty<object>() } // impossible: correct > total
        },
        questionStats = new object[]
        {
            new { questionId = "sample-mcq-junior", seen = 3, correct = 2, lastCorrect = false, lastAt = "2026-03-01T10:00:00.000Z" },
            new { questionId = "does-not-exist", seen = 1, correct = 1, lastCorrect = true, lastAt = "2026-03-01T10:00:00.000Z" }
        },
        lessons = new object[]
        {
            new { lessonId = "sample-lesson", answers = new Dictionary<string, bool> { ["e1"] = true, ["nope"] = true } },
            new { lessonId = "no-such-lesson", answers = new Dictionary<string, bool> { ["e1"] = true } }
        }
    };

    [Fact]
    public async Task Import_adds_valid_local_progress_and_skips_the_rest()
    {
        var session = await Accounts.Register(factory);
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Post, "/api/me/progress/import", LocalProgress())).StatusCode);

        var progress = await session.GetJson("/api/me/progress");
        Assert.Equal(1, progress.GetProperty("history").GetArrayLength());
        Assert.Equal(70, progress.GetProperty("history")[0].GetProperty("percent").GetInt32());

        var stat = Assert.Single(progress.GetProperty("questionStats").EnumerateArray());
        Assert.Equal("sample-mcq-junior", stat.GetProperty("questionId").GetString());
        Assert.Equal(3, stat.GetProperty("seen").GetInt32());

        var lesson = Assert.Single(progress.GetProperty("lessons").EnumerateArray());
        Assert.Equal(["e1"], lesson.GetProperty("answers").EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public async Task Importing_the_same_history_twice_does_not_duplicate_it()
    {
        var session = await Accounts.Register(factory);
        await session.Send(HttpMethod.Post, "/api/me/progress/import", LocalProgress());
        await session.Send(HttpMethod.Post, "/api/me/progress/import", LocalProgress());

        Assert.Equal(1, (await session.GetJson("/api/me/progress")).GetProperty("history").GetArrayLength());
    }

    [Fact]
    public async Task Import_keeps_the_answers_the_server_already_has()
    {
        var session = await Accounts.Register(factory);
        await session.Send(HttpMethod.Post, "/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = 0 });
        var onServer = (await session.GetJson("/api/me/progress")).GetProperty("lessons")[0].GetProperty("answers").GetProperty("e1").GetBoolean();

        await session.Send(HttpMethod.Post, "/api/me/progress/import", new
        {
            lessons = new object[] { new { lessonId = "sample-lesson", answers = new Dictionary<string, bool> { ["e1"] = !onServer } } }
        });

        var after = (await session.GetJson("/api/me/progress")).GetProperty("lessons")[0].GetProperty("answers").GetProperty("e1").GetBoolean();
        Assert.Equal(onServer, after);
    }

    [Fact]
    public async Task Import_rejects_oversized_requests()
    {
        var session = await Accounts.Register(factory);
        var tooMany = Enumerable.Range(0, 201).Select(i => new { at = "2026-03-01T10:00:00Z", mode = "Learning", total = 1, correct = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, (await session.Send(HttpMethod.Post, "/api/me/progress/import", new { history = tooMany })).StatusCode);
    }

    [Fact]
    public async Task Clear_removes_the_progress_but_not_the_account()
    {
        var session = await Accounts.Register(factory);
        await session.Send(HttpMethod.Post, "/api/practice/evaluate", await Answers(session.Client, true));
        await session.Send(HttpMethod.Post, "/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = 0 });

        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Delete, "/api/me/progress")).StatusCode);

        var progress = await session.GetJson("/api/me/progress");
        Assert.Equal(0, progress.GetProperty("history").GetArrayLength());
        Assert.Equal(0, progress.GetProperty("questionStats").GetArrayLength());
        Assert.Equal(0, progress.GetProperty("lessons").GetArrayLength());
        Assert.Equal(HttpStatusCode.OK, (await session.Send(HttpMethod.Get, "/api/auth/me")).StatusCode);
    }
}

/// <summary>Guards the PostgreSQL migrations: the model must not change without a new migration.</summary>
public class MigrationTests
{
    [Fact]
    public void The_postgres_migrations_match_the_model()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=localhost;Database=never-connected").Options;
        using var db = new AppDbContext(options);

        Assert.False(db.Database.HasPendingModelChanges(),
            "The model changed. Add a migration: dotnet ef migrations add <Name> --project src/InterviewPal.Infrastructure --output-dir Persistence/Migrations");
        Assert.NotEmpty(db.Database.GetMigrations());
    }

    [Theory]
    [InlineData("postgres://user:p%40ss@db.example.com:5432/shop", "Host=db.example.com;Port=5432;Database=shop;Username=user;Password=p@ss;SSL Mode=Require")]
    [InlineData("postgresql://u:pw@host/db?sslmode=disable", "Host=host;Port=5432;Database=db;Username=u;Password=pw;SSL Mode=Disable")]
    [InlineData("postgresql://u:pw@h-pooler.neon.tech/neondb?sslmode=require&channel_binding=require", "Host=h-pooler.neon.tech;Port=5432;Database=neondb;Username=u;Password=pw;SSL Mode=Require;Channel Binding=Require")]
    [InlineData("Host=x;Database=y", "Host=x;Database=y")]
    public void Hosted_database_urls_become_connection_strings(string url, string expected) =>
        Assert.Equal(expected, PostgresConnection.Normalize(url));
}
