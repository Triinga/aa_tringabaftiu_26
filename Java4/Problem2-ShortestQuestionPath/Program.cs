var questionnaire = CreateQuestionnaire();
PathResult shortestPath = FindShortestQuestionPath(questionnaire);

Console.WriteLine($"Length: {shortestPath.Length}");
Console.WriteLine($"Questions: {string.Join(" -> ", shortestPath.QuestionIds)}");
Console.WriteLine($"Answers: {string.Join(" -> ", shortestPath.AnswerIds)}");

static PathResult FindShortestQuestionPath(Questionnaire questionnaire)
{
    var questionsOnPath = new HashSet<string>();

    return FindShortestFrom(questionnaire.StartQuestionId, questionnaire, questionsOnPath)
        ?? throw new InvalidOperationException("No complete path was found.");
}

static PathResult? FindShortestFrom(
    string questionId,
    Questionnaire questionnaire,
    HashSet<string> questionsOnPath)
{
    Question? question = questionnaire.Questions
        .FirstOrDefault(item => item.Id == questionId);

    if (question is null)
        throw new InvalidOperationException($"Question '{questionId}' does not exist.");
    if (questionsOnPath.Contains(questionId))
        return null;

    questionsOnPath.Add(questionId);
    PathResult? bestPath = null;

    if (question.Type == "radio" || question.Type == "select")
    {
        foreach (Answer answer in question.Answers)
        {
            PathResult? candidatePath;

            if (answer.NextQuestionId is null)
            {
                candidatePath = new PathResult(
                    1,
                    new List<string> { question.Id },
                    new List<string> { answer.Id });
            }
            else
            {
                PathResult? nextPath = FindShortestFrom(
                    answer.NextQuestionId,
                    questionnaire,
                    questionsOnPath);

                if (nextPath is null)
                    continue;

                candidatePath = new PathResult(
                    nextPath.Length + 1,
                    new List<string> { question.Id },
                    new List<string> { answer.Id });

                candidatePath.QuestionIds.AddRange(nextPath.QuestionIds);
                candidatePath.AnswerIds.AddRange(nextPath.AnswerIds);
            }

            if (bestPath is null || candidatePath.Length < bestPath.Length)
                bestPath = candidatePath;
        }
    }
    else
    {
        if (question.NextQuestionId is null)
        {
            bestPath = new PathResult(
                1,
                new List<string> { question.Id },
                new List<string>());
        }
        else
        {
            PathResult? nextPath = FindShortestFrom(
                question.NextQuestionId,
                questionnaire,
                questionsOnPath);

            if (nextPath is not null)
            {
                bestPath = new PathResult(
                    nextPath.Length + 1,
                    new List<string> { question.Id },
                    new List<string>(nextPath.AnswerIds));

                bestPath.QuestionIds.AddRange(nextPath.QuestionIds);
            }
        }
    }
    questionsOnPath.Remove(questionId);
    return bestPath;
}

static Questionnaire CreateQuestionnaire()
{
    return new Questionnaire("q1", new List<Question>
    {
        new Question("q1", "Do you own a car?", "radio", new List<Answer>
        {
            new Answer("a1_yes", "Yes", "q2"),
            new Answer("a1_no", "No", "q5")
        }),
        new Question("q2", "What type of car do you own?", "select", new List<Answer>
        {
            new Answer("a2_sedan", "Sedan", "q3"),
            new Answer("a2_suv", "SUV", "q3"),
            new Answer("a2_truck", "Truck", "q4"),
            new Answer("a2_sports", "Sports Car", "q4")
        }),
        new Question("q3", "How many passengers can your vehicle seat?", "radio", new List<Answer>
        {
            new Answer("a3_2to5", "2-5", "q6"),
            new Answer("a3_6plus", "6+", "q6")
        }),
        new Question("q4", "Do you use your vehicle for work purposes?", "radio", new List<Answer>
        {
            new Answer("a4_yes", "Yes", "q7"),
            new Answer("a4_no", "No", "q6")
        }),
        new Question("q5", "What is your primary mode of transportation?", "select", new List<Answer>
        {
            new Answer("a5_public", "Public Transit", "q6"),
            new Answer("a5_bike", "Bicycle", "q6"),
            new Answer("a5_walk", "Walking", "q6")
        }),
        new Question("q6", "What is your annual commute distance?", "text", new List<Answer>(), "q8"),
        new Question("q7", "What percentage of your vehicle use is for work?", "text", new List<Answer>(), "q8"),
        new Question("q8", "Are you satisfied with your current transportation situation?", "radio", new List<Answer>
        {
            new Answer("a8_yes", "Yes", null),
            new Answer("a8_no", "No", "q9")
        }),
        new Question("q9", "What would you like to improve?", "text", new List<Answer>())
    });
}

record Answer(string Id, string Text, string? NextQuestionId);
record Question(
    string Id,
    string Text,
    string Type,
    List<Answer> Answers,
    string? NextQuestionId = null);
record Questionnaire(string StartQuestionId, List<Question> Questions);
record PathResult(int Length, List<string> QuestionIds, List<string> AnswerIds);
