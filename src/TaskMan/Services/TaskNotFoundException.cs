namespace TaskMan.Services;

public class TaskNotFoundException(int id) : Exception($"No task found with id {id}.")
{
    public int TaskId { get; } = id;
}
