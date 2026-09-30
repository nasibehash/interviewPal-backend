using InterviewPal.Domain;

namespace InterviewPal.Application.Abstractions;

/// <summary>Read-only access to the lessons (algorithms and design patterns).</summary>
public interface ILessonCatalog
{
    IReadOnlyList<Lesson> All { get; }
}
