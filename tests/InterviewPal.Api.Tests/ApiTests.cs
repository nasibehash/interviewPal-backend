using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Api.Tests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private async Task<JsonElement> GetJson(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    private async Task<JsonElement> PostJson(string url, object body, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await _client.PostAsJsonAsync(url, body, Json);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var body = await GetJson("/health");
        Assert.Equal("ok", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Technologies_include_counts_per_level()
    {
        var techs = await GetJson("/api/technologies");
        var sample = techs.EnumerateArray().Single(t => t.GetProperty("slug").GetString() == "sample");
        Assert.Equal(3, sample.GetProperty("questionCount").GetInt32());
        Assert.Equal("1", sample.GetProperty("supportedFrom").GetString());
        Assert.Equal("3", sample.GetProperty("currentVersion").GetString());
        Assert.Equal(1, sample.GetProperty("countByLevel").GetProperty("Junior").GetInt32());
    }

    [Fact]
    public async Task Question_list_hides_answers_and_correct_flags()
    {
        var raw = await _client.GetStringAsync("/api/questions?technology=sample");
        Assert.DoesNotContain("isCorrect", raw);
        Assert.DoesNotContain("shortAnswer", raw);
        Assert.DoesNotContain("explanation", raw);
    }

    [Theory]
    [InlineData("level=Junior", 1)]
    [InlineData("level=senior", 1)]
    [InlineData("tag=basics", 2)]
    [InlineData("tag=alpha", 1)]
    [InlineData("search=print", 1)]
    [InlineData("technology=sample&pageSize=2", 3)]
    public async Task Question_list_filters(string query, int expectedTotal)
    {
        var page = await GetJson($"/api/questions?{query}");
        Assert.Equal(expectedTotal, page.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Question_list_rejects_unknown_level()
    {
        var response = await _client.GetAsync("/api/questions?level=Guru");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Question_detail_includes_answer()
    {
        var detail = await GetJson("/api/questions/sample-mcq-junior");
        var answer = detail.GetProperty("answer");
        Assert.Equal("B", answer.GetProperty("shortAnswer").GetString());
        Assert.Equal(1, answer.GetProperty("correctChoiceIds").GetArrayLength());
    }

    [Fact]
    public async Task Unknown_question_returns_404_problem_details()
    {
        var response = await _client.GetAsync("/api/questions/nope");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Practice_session_respects_filters_and_is_reproducible_with_seed()
    {
        var request = new { technologies = new[] { "sample" }, levels = new[] { "Junior", "Mid" }, count = 5, mode = "Interview", seed = 7 };
        var a = await PostJson("/api/practice/sessions", request);
        var b = await PostJson("/api/practice/sessions", request);

        Assert.Equal(2, a.GetProperty("totalQuestions").GetInt32());
        Assert.Equal(105, a.GetProperty("timeLimitSeconds").GetInt32());
        Assert.Equal(2, a.GetProperty("estimatedMinutes").GetInt32());
        Assert.Equal(a.GetProperty("questions").ToString(), b.GetProperty("questions").ToString());
    }

    [Fact]
    public async Task Practice_session_rejects_out_of_range_count()
    {
        await PostJson("/api/practice/sessions", new { count = 2 }, HttpStatusCode.BadRequest);
        await PostJson("/api/practice/sessions", new { count = 101 }, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Check_grades_choice_and_self_assessed_questions()
    {
        var detail = await GetJson("/api/questions/sample-mcq-junior");
        var correct = detail.GetProperty("answer").GetProperty("correctChoiceIds")[0].GetInt32();
        var wrong = detail.GetProperty("question").GetProperty("choices").EnumerateArray()
            .Select(c => c.GetProperty("id").GetInt32()).First(id => id != correct);

        var ok = await PostJson("/api/practice/questions/sample-mcq-junior/check", new { choiceId = correct });
        var bad = await PostJson("/api/practice/questions/sample-mcq-junior/check", new { choiceId = wrong });
        var flash = await PostJson("/api/practice/questions/sample-short-senior/check", new { knewIt = false });

        Assert.True(ok.GetProperty("isCorrect").GetBoolean());
        Assert.False(bad.GetProperty("isCorrect").GetBoolean());
        Assert.False(flash.GetProperty("isCorrect").GetBoolean());
        Assert.Equal("Because B.", bad.GetProperty("answer").GetProperty("explanation").GetString());
    }

    [Fact]
    public async Task Check_rejects_missing_or_foreign_choice()
    {
        await PostJson("/api/practice/questions/sample-mcq-junior/check", new { }, HttpStatusCode.BadRequest);
        await PostJson("/api/practice/questions/sample-mcq-junior/check", new { choiceId = 999999 }, HttpStatusCode.BadRequest);
        await PostJson("/api/practice/questions/sample-short-senior/check", new { }, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Evaluate_scores_by_technology_level_and_weak_tags()
    {
        var mcq = await GetJson("/api/questions/sample-mcq-junior");
        var code = await GetJson("/api/questions/sample-code-mid");
        int Correct(JsonElement d) => d.GetProperty("answer").GetProperty("correctChoiceIds")[0].GetInt32();
        int Wrong(JsonElement d) => d.GetProperty("question").GetProperty("choices").EnumerateArray()
            .Select(c => c.GetProperty("id").GetInt32()).First(id => id != Correct(d));

        var result = await PostJson("/api/practice/evaluate", new
        {
            answers = new object[]
            {
                new { questionId = "sample-mcq-junior", choiceId = Correct(mcq) },
                new { questionId = "sample-code-mid", choiceId = Wrong(code) },
                new { questionId = "sample-short-senior", knewIt = false }
            }
        });

        Assert.Equal(3, result.GetProperty("total").GetInt32());
        Assert.Equal(1, result.GetProperty("correct").GetInt32());
        Assert.Equal(33, result.GetProperty("percent").GetInt32());
        Assert.Equal(new string?[] { "sample-code-mid", "sample-short-senior" },
            result.GetProperty("weakQuestionIds").EnumerateArray().Select(e => e.GetString()).ToArray());
        Assert.Contains(result.GetProperty("weakTags").EnumerateArray(), t => t.GetProperty("key").GetString() == "deep");
        Assert.Equal(3, result.GetProperty("byLevel").GetArrayLength());
    }

    [Fact]
    public async Task Evaluate_rejects_duplicates_empty_and_unknown_questions()
    {
        await PostJson("/api/practice/evaluate", new { answers = Array.Empty<object>() }, HttpStatusCode.BadRequest);
        await PostJson("/api/practice/evaluate", new
        {
            answers = new[] { new { questionId = "sample-short-senior", knewIt = true }, new { questionId = "sample-short-senior", knewIt = true } }
        }, HttpStatusCode.BadRequest);
        await PostJson("/api/practice/evaluate", new
        {
            answers = new[] { new { questionId = "ghost", knewIt = true } }
        }, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Report_is_stored_and_validated()
    {
        var response = await _client.PostAsJsonAsync("/api/questions/sample-mcq-junior/reports",
            new { reason = "WrongAnswer", message = "  B is not right  " }, Json);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var stored = await factory.WithDbAsync(db => db.QuestionReports.SingleAsync(r => r.QuestionId == "sample-mcq-junior"));
        Assert.Equal(ReportReason.WrongAnswer, stored.Reason);
        Assert.Equal("B is not right", stored.Message);

        await PostJson("/api/questions/sample-mcq-junior/reports", new { message = "no reason" }, HttpStatusCode.BadRequest);
        await PostJson("/api/questions/ghost/reports", new { reason = "Typo" }, HttpStatusCode.NotFound);
    }
}
