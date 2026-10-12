var questionnaire = CreateQuestionnaire();
List<QuestionProbability> results = CalculateQuestionProbabilities(questionnaire);

foreach (QuestionProbability result in results)
{
    Console.WriteLine(
        $"{result.QuestionId}: probability={result.Probability:F4}, " +
        $"expected position={result.ExpectedPosition:F4} - {result.QuestionText}");
}

static List<QuestionProbability> CalculateQuestionProbabilities(Questionnaire questionnaire)
{
    var probabilities = new Dictionary<string, double>();
    var positionTotals = new Dictionary<string, double>();

    void Visit(string questionId, double probability, int position)
    {
        Question? question = questionnaire.Questions
            .FirstOrDefault(item => item.Id == questionId);

        if (question is null)
            throw new InvalidOperationException($"Question '{questionId}' does not exist.");
        if (!probabilities.ContainsKey(questionId))
        {
            probabilities[questionId] = 0;
            positionTotals[questionId] = 0;
        }

        probabilities[questionId] += probability;
        positionTotals[questionId] += probability * position;

        if (question.Type == "radio" || question.Type == "select")
        {
            int answerCount = question.Answers.Count;
            if (answerCount == 0)
                return;

            double probabilityPerAnswer = probability / answerCount;

            foreach (Answer answer in question.Answers)
            {
                if (answer.NextQuestionId is not null)
                {
                    Visit(answer.NextQuestionId, probabilityPerAnswer, position + 1);
                }
            }
        }
        else if (question.NextQuestionId is not null)
        {
            Visit(question.NextQuestionId, probability, position + 1);
        }
    }
    Visit(questionnaire.StartQuestionId, 1.0, 1);

    var results = new List<QuestionProbability>();

    foreach (Question question in questionnaire.Questions)
    {
        double probability = probabilities.GetValueOrDefault(question.Id, 0.0);
        double expectedPosition = probability == 0
            ? 0.0
            : positionTotals[question.Id] / probability;

        results.Add(new QuestionProbability(
            question.Id,
            question.Text,
            Math.Round(probability, 4),
            Math.Round(expectedPosition, 4)));
    }

    return results
        .OrderByDescending(result => result.Probability)
        .ThenBy(result => result.QuestionId)
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
record QuestionProbability(
    string QuestionId,
    string QuestionText,
    double Probability,
    double ExpectedPosition);
