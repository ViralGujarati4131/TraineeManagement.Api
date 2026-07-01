namespace TraineeManagement.Api.Data.CacheKey;

public static class CacheKey
{
    public static string Trainee(int id) => $"trainee:{id}";

    public static string TaskAssignment(int id) => $"task-assignment:{id}";

    public static string Submission(int id) => $"submission:{id}";

    public static string AllLearningTask() => $"learnig-task:All";

    public static string AllMentor() => $"mentor:All";

    public static string AllReview() => $"review:All";

}

public static class CacheTime
{
    public static int TTL = 10;
}