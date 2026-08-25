using System.Text.Json;
using System.Text.Json.Serialization;
using TaskMan.Models;

namespace TaskMan.Services;

/// <summary>
/// Persists tasks as a single JSON file. Simple on purpose — a real app would
/// swap this for a database, but the TaskService above doesn't need to know
/// or care which storage is behind it.
/// </summary>
public class TaskRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;

    public TaskRepository(string path)
    {
        _path = path;
    }

    public List<TaskItem> Load()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        var json = File.ReadAllText(_path);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<TaskItem>>(json, JsonOptions) ?? [];
    }

    public void Save(IEnumerable<TaskItem> tasks)
    {
        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_path, JsonSerializer.Serialize(tasks, JsonOptions));
    }
}
