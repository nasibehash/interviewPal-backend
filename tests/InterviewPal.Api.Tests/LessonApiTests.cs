using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InterviewPal.Api.Tests;

public class LessonApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = Accounts.LoggedInClient(factory);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private async Task<JsonElement> Get(string url, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    [Fact]
    public async Task Lists_lessons_and_filters_by_kind_and_technology()
    {
        var all = await Get("/api/lessons");
        Assert.Equal("sample-lesson", all[0].GetProperty("id").GetString());
        Assert.Equal("Algorithm", all[0].GetProperty("kind").GetString());

        Assert.Equal(0, (await Get("/api/lessons?kind=DesignPattern")).GetArrayLength());
        Assert.Equal(1, (await Get("/api/lessons?technology=sample")).GetArrayLength());
        Assert.Equal(0, (await Get("/api/lessons?technology=react")).GetArrayLength());
        await Get("/api/lessons?kind=nope", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Detail_contains_the_implementation_for_the_chosen_technology_but_no_answers()
    {
        var lesson = await Get("/api/lessons/sample-lesson?technology=sample");
        Assert.Equal("javascript", lesson.GetProperty("implementation").GetProperty("language").GetString());
        Assert.Equal("export const sample = (x) => x;", lesson.GetProperty("implementation").GetProperty("code").GetString());
        Assert.Equal(["testing", "sample"], lesson.GetProperty("tags").EnumerateArray().Select(t => t.GetString()));

        var exercise = lesson.GetProperty("exercises")[0];
        Assert.False(exercise.TryGetProperty("explanation", out _));
        Assert.False(exercise.GetProperty("choices")[0].TryGetProperty("isCorrect", out _));
    }

    [Fact]
    public async Task Detail_without_technology_has_no_implementation_and_unknown_technology_is_rejected()
    {
        var lesson = await Get("/api/lessons/sample-lesson");
        Assert.Equal(JsonValueKind.Null, lesson.GetProperty("implementation").ValueKind);
        await Get("/api/lessons/sample-lesson?technology=react", HttpStatusCode.BadRequest);
        await Get("/api/lessons/missing", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Checks_exercises()
    {
        var lesson = await Get("/api/lessons/sample-lesson?technology=sample");
        var choices = lesson.GetProperty("exercises")[0].GetProperty("choices").EnumerateArray().ToList();
        var right = choices.Single(c => c.GetProperty("text").GetString() == "right").GetProperty("id").GetInt32();
        var wrong = choices.First(c => c.GetProperty("text").GetString() == "wrong 1").GetProperty("id").GetInt32();

        var ok = await _client.PostAsJsonAsync("/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = right });
        var okBody = await ok.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(okBody.GetProperty("isCorrect").GetBoolean());
        Assert.Equal("Because it is right.", okBody.GetProperty("explanation").GetString());

        var bad = await _client.PostAsJsonAsync("/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = wrong });
        var badBody = await bad.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.False(badBody.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(right, badBody.GetProperty("correctChoiceId").GetInt32());
    }

    [Fact]
    public async Task Check_rejects_unknown_exercises_unknown_choices_and_missing_bodies()
    {
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.PostAsJsonAsync("/api/lessons/sample-lesson/exercises/zzz/check", new { choiceId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync("/api/lessons/sample-lesson/exercises/e1/check", new { choiceId = 99 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync("/api/lessons/sample-lesson/exercises/e1/check", new { })).StatusCode);
    }
}
