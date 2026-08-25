using TaskMan.Models;
using TaskMan.Services;

namespace TaskMan.Tests;

public class TaskServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"taskman-tests-{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    private TaskService CreateService() => new(new TaskRepository(_path));

    [Fact]
    public void Add_AssignsIncrementingIds()
    {
        var service = CreateService();

        var first = service.Add("First task", Priority.Medium, null);
        var second = service.Add("Second task", Priority.Medium, null);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void Add_PersistsAcrossServiceInstances()
    {
        CreateService().Add("Persisted task", Priority.High, null);

        var task = CreateService().Find(1);

        Assert.Equal("Persisted task", task.Title);
        Assert.Equal(Priority.High, task.Priority);
    }

    [Fact]
    public void Complete_MarksTaskAsDone()
    {
        var service = CreateService();
        var task = service.Add("Wash the car", Priority.Low, null);

        var completed = service.Complete(task.Id);

        Assert.Equal(TaskState.Done, completed.State);
    }

    [Fact]
    public void Complete_UnknownId_ThrowsTaskNotFoundException()
    {
        var service = CreateService();

        Assert.Throws<TaskNotFoundException>(() => service.Complete(999));
    }

    [Fact]
    public void Remove_DeletesTask()
    {
        var service = CreateService();
        var task = service.Add("Temporary task", Priority.Medium, null);

        service.Remove(task.Id);

        Assert.Throws<TaskNotFoundException>(() => service.Find(task.Id));
    }

    [Fact]
    public void List_FiltersByStatus()
    {
        var service = CreateService();
        var pending = service.Add("Pending task", Priority.Medium, null);
        var done = service.Add("Done task", Priority.Medium, null);
        service.Complete(done.Id);

        var pendingOnly = service.List(TaskState.Pending, null);

        Assert.Single(pendingOnly);
        Assert.Equal(pending.Id, pendingOnly[0].Id);
    }

    [Fact]
    public void List_SortsByPriorityDescending()
    {
        var service = CreateService();
        service.Add("Low priority", Priority.Low, null);
        service.Add("High priority", Priority.High, null);
        service.Add("Medium priority", Priority.Medium, null);

        var sorted = service.List(null, "priority");

        Assert.Equal(Priority.High, sorted[0].Priority);
        Assert.Equal(Priority.Medium, sorted[1].Priority);
        Assert.Equal(Priority.Low, sorted[2].Priority);
    }

    [Fact]
    public void List_SortsByDueDateWithUndatedTasksLast()
    {
        var service = CreateService();
        service.Add("No due date", Priority.Medium, null);
        service.Add("Due soon", Priority.Medium, new DateOnly(2026, 1, 1));

        var sorted = service.List(null, "due");

        Assert.Equal("Due soon", sorted[0].Title);
        Assert.Equal("No due date", sorted[1].Title);
    }
}
