using TaskMan.Models;

namespace TaskMan.Services;

public class TaskService
{
    private readonly TaskRepository _repository;
    private readonly List<TaskItem> _tasks;

    public TaskService(TaskRepository repository)
    {
        _repository = repository;
        _tasks = repository.Load();
    }

    public TaskItem Add(string title, Priority priority, DateOnly? dueDate)
    {
        var id = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;

        var task = new TaskItem
        {
            Id = id,
            Title = title,
            Priority = priority,
            DueDate = dueDate,
            CreatedAt = DateTimeOffset.Now,
        };

        _tasks.Add(task);
        _repository.Save(_tasks);

        return task;
    }

    public TaskItem Complete(int id)
    {
        var task = Find(id);
        task.State = TaskState.Done;
        _repository.Save(_tasks);

        return task;
    }

    public void Remove(int id)
    {
        var task = Find(id);
        _tasks.Remove(task);
        _repository.Save(_tasks);
    }

    public TaskItem Find(int id)
    {
        return _tasks.FirstOrDefault(t => t.Id == id) ?? throw new TaskNotFoundException(id);
    }

    /// <summary>
    /// statusFilter of null means "all statuses" — that's what lets `list --status all` work.
    /// </summary>
    public IReadOnlyList<TaskItem> List(TaskState? statusFilter, string? sortBy)
    {
        IEnumerable<TaskItem> query = _tasks;

        if (statusFilter is not null)
        {
            query = query.Where(t => t.State == statusFilter);
        }

        query = sortBy switch
        {
            "due" => query.OrderBy(t => t.DueDate ?? DateOnly.MaxValue),
            "priority" => query.OrderByDescending(t => t.Priority),
            _ => query.OrderBy(t => t.Id),
        };

        return query.ToList();
    }
}
