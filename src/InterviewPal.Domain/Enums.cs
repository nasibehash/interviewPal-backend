namespace InterviewPal.Domain;

public enum Level
{
    Junior = 1,
    Mid = 2,
    Senior = 3
}

public enum QuestionType
{
    /// <summary>Pick one of several options.</summary>
    MultipleChoice = 1,

    /// <summary>"What is the output of this code?" - options are provided.</summary>
    CodeOutput = 2,

    /// <summary>Short free-form answer, self-assessed by the learner.</summary>
    ShortAnswer = 3,

    /// <summary>Conceptual explanation, self-assessed by the learner.</summary>
    Conceptual = 4
}

public enum ReportReason
{
    WrongAnswer = 1,
    Outdated = 2,
    Unclear = 3,
    Typo = 4,
    Other = 5
}
