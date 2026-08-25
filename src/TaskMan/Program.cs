using TaskMan.Cli;
using TaskMan.Models;
using TaskMan.Services;

var dataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TaskMan",
    "tasks.json");

var service = new TaskService(new TaskRepository(dataPath));

if (args.Length == 0)
{
    PrintHelp();
    return 0;
}

try
{
    return args[0].ToLowerInvariant() switch
    {
        "add" => RunAdd(args),
        "list" => RunList(args),
        "done" => RunDone(args),
        "remove" => RunRemove(args),
        "help" or "--help" or "-h" => RunHelp(),
        var unknown => RunUnknown(unknown),
    };
}
catch (TaskNotFoundException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

int RunAdd(string[] a)
{
    var title = a.Skip(1).FirstOrDefault(x => !x.StartsWith("--"));

    if (string.IsNullOrWhiteSpace(title))
    {
        Console.Error.WriteLine("Usage: taskman add \"<title>\" [--priority low|medium|high] [--due yyyy-MM-dd]");
        return 1;
    }

    var priority = Priority.Medium;
    var (priorityValue, hasPriority) = ArgumentParser.GetOption(a, "--priority");

    if (hasPriority && !Enum.TryParse(priorityValue, ignoreCase: true, out priority))
    {
        Console.Error.WriteLine($"Invalid priority: {priorityValue}. Use low, medium, or high.");
        return 1;
    }

    DateOnly? dueDate = null;
    var (dueValue, hasDue) = ArgumentParser.GetOption(a, "--due");

    if (hasDue)
    {
        if (!DateOnly.TryParse(dueValue, out var parsedDue))
        {
            Console.Error.WriteLine($"Invalid due date: {dueValue}. Use yyyy-MM-dd.");
            return 1;
        }

        dueDate = parsedDue;
    }

    var task = service.Add(title, priority, dueDate);
    Console.WriteLine($"Added task #{task.Id}: {task.Title}");

    return 0;
}

int RunList(string[] a)
{
    TaskState? statusFilter = TaskState.Pending;
    var (statusValue, hasStatus) = ArgumentParser.GetOption(a, "--status");

    if (hasStatus)
    {
        switch (statusValue?.ToLowerInvariant())
        {
            case "all":
                statusFilter = null;
                break;
            case "done":
                statusFilter = TaskState.Done;
                break;
            case "pending":
                statusFilter = TaskState.Pending;
                break;
            default:
                Console.Error.WriteLine($"Invalid status: {statusValue}. Use pending, done, or all.");
                return 1;
        }
    }

    var (sortBy, _) = ArgumentParser.GetOption(a, "--sort");
    var tasks = service.List(statusFilter, sortBy);

    if (tasks.Count == 0)
    {
        Console.WriteLine("No tasks found.");
        return 0;
    }

    foreach (var task in tasks)
    {
        var marker = task.State == TaskState.Done ? "x" : " ";
        var due = task.DueDate is { } d ? $" (due {d:yyyy-MM-dd})" : "";
        Console.WriteLine($"[{marker}] #{task.Id} [{task.Priority}] {task.Title}{due}");
    }

    return 0;
}

int RunDone(string[] a)
{
    if (a.Length < 2 || !int.TryParse(a[1], out var id))
    {
        Console.Error.WriteLine("Usage: taskman done <id>");
        return 1;
    }

    var task = service.Complete(id);
    Console.WriteLine($"Completed task #{task.Id}: {task.Title}");

    return 0;
}

int RunRemove(string[] a)
{
    if (a.Length < 2 || !int.TryParse(a[1], out var id))
    {
        Console.Error.WriteLine("Usage: taskman remove <id>");
        return 1;
    }

    service.Remove(id);
    Console.WriteLine($"Removed task #{id}");

    return 0;
}

int RunHelp()
{
    PrintHelp();
    return 0;
}

int RunUnknown(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintHelp();

    return 1;
}

void PrintHelp()
{
    Console.WriteLine("""
        TaskMan - a simple command-line task manager

        Usage:
          taskman add "<title>" [--priority low|medium|high] [--due yyyy-MM-dd]
          taskman list [--status pending|done|all] [--sort due|priority]
          taskman done <id>
          taskman remove <id>
          taskman help

        Tasks are stored in %AppData%\TaskMan\tasks.json
        """);
}
