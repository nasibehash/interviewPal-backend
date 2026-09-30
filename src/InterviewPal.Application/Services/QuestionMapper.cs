using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

internal static class QuestionMapper
{
    public static QuestionDto ToDto(Question q) => new(
        q.Id,
        q.TechnologySlug,
        q.Level.ToString(),
        q.Type.ToString(),
        q.Text,
        q.CodeSnippet,
        q.CodeLanguage,
        q.EstimatedSeconds,
        q.MinVersion,
        q.MaxVersion,
        q.Tags,
        q.Choices.OrderBy(c => c.Order).Select(c => new ChoiceDto(c.Id, c.Text)).ToList());

    public static AnswerDto ToAnswer(Question q) => new(
        q.Choices.Where(c => c.IsCorrect).Select(c => c.Id).ToList(),
        q.ShortAnswer,
        q.Explanation,
        q.CommonMistake,
        q.FollowUpQuestion);

    public static bool HasChoices(Question q) =>
        q.Type is QuestionType.MultipleChoice or QuestionType.CodeOutput;
}
