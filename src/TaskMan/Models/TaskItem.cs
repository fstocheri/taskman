namespace TaskMan.Models;

public class TaskItem
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public TaskState State { get; set; } = TaskState.Pending;

    public DateOnly? DueDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
