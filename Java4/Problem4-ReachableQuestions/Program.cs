var questionnaire = CreateQuestionnaire();
List<ReachableQuestion> reachableQuestions = FindReachableQuestions(
    questionnaire,
    "q2",
    "a2_truck");

foreach (ReachableQuestion question in reachableQuestions)
{
    Console.WriteLine($"{question.QuestionId}: depth {question.MinDepth} - {question.QuestionText}");
}

static List<ReachableQuestion> FindReachableQuestions(
    Questionnaire questionnaire,
    string questionId,
    string answerId)
{
    Question? startingQuestion = questionnaire.Questions
        .FirstOrDefault(question => question.Id == questionId);

    if (startingQuestion is null)
        return new List<ReachableQuestion>();

    Answer? selectedAnswer = startingQuestion.Answers
        .FirstOrDefault(answer => answer.Id == answerId);

    if (selectedAnswer?.NextQuestionId is null)
        return new List<ReachableQuestion>();
    var minimumDepths = new Dictionary<string, int>();

    void Visit(string currentQuestionId, int depth)
    {
        if (minimumDepths.TryGetValue(currentQuestionId, out int oldDepth)
            && oldDepth <= depth)
        {
            return;
        }

        minimumDepths[currentQuestionId] = depth;

        Question? currentQuestion = questionnaire.Questions
            .FirstOrDefault(question => question.Id == currentQuestionId);

        if (currentQuestion is null)
            return;

        if (currentQuestion.Type == "radio" || currentQuestion.Type == "select")
        {
            foreach (Answer answer in currentQuestion.Answers)
            {
                if (answer.NextQuestionId is not null)
                    Visit(answer.NextQuestionId, depth + 1);
            }
        }
        else if (currentQuestion.NextQuestionId is not null)
        {
            Visit(currentQuestion.NextQuestionId, depth + 1);
        }
    }
    Visit(selectedAnswer.NextQuestionId, 1);

    return minimumDepths
        .Where(item => questionnaire.Questions.Any(question => question.Id == item.Key))
        .Select(item =>
        {
            Question question = questionnaire.Questions
                .First(question => question.Id == item.Key);
            return new ReachableQuestion(question.Id, question.Text, item.Value);
        })
        .OrderBy(question => question.MinDepth)
        .ThenBy(question => question.QuestionId)
        .ToList();
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
record ReachableQuestion(string QuestionId, string QuestionText, int MinDepth);
