# TaskMan

A small command-line task manager, written in C# / .NET 10. Built as a focused, standalone
demo of core C# fundamentals: a clean layered design (models / repository / service / CLI),
LINQ, JSON persistence, custom exceptions, and unit tests — nothing more than that, on purpose.

## Usage

```bash
taskman add "<title>" [--priority low|medium|high] [--due yyyy-MM-dd]
taskman list [--status pending|done|all] [--sort due|priority]
taskman done <id>
taskman remove <id>
taskman help
```

Tasks are stored as JSON at `%AppData%\TaskMan\tasks.json` (or the OS equivalent), so the tool
behaves the same regardless of which directory you run it from.

### Example

```bash
$ taskman add "Write the README" --priority high --due 2026-09-01
Added task #1: Write the README

$ taskman add "Buy groceries" --priority low
Added task #2: Buy groceries

$ taskman list
[ ] #1 [High] Write the README (due 2026-09-01)
[ ] #2 [Low] Buy groceries

$ taskman done 2
Completed task #2: Buy groceries

$ taskman list --status all --sort priority
[ ] #1 [High] Write the README (due 2026-09-01)
[x] #2 [Low] Buy groceries
```

## Design

- **`Models/`** — `TaskItem`, and the `Priority` / `TaskState` enums. Plain data, no behavior.
- **`Services/TaskRepository.cs`** — the only class that knows tasks live in a JSON file.
  Swapping this for a database later wouldn't touch anything else.
- **`Services/TaskService.cs`** — the actual business logic (add/complete/remove/list,
  id assignment, filtering and sorting via LINQ). Throws a custom `TaskNotFoundException`
  rather than returning null/bool, so callers can't silently ignore a bad id.
- **`Cli/`** + **`Program.cs`** — argument parsing and command dispatch. Kept deliberately
  dependency-free (no argument-parsing library) since the surface area is small enough that
  a library would be more ceremony than the problem warrants.

## Running it

```bash
git clone https://github.com/ftocheri/taskman.git
cd taskman
dotnet run --project src/TaskMan -- add "Try this out"
dotnet run --project src/TaskMan -- list
```

## Tests

```bash
dotnet test
```

Covers id assignment, persistence across separate `TaskService` instances (proving the JSON
round-trip actually works, not just the in-memory list), completing/removing tasks, the
not-found exception path, and both list filters (status) and sorts (priority, due date).
