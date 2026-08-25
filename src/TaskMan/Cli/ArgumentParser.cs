namespace TaskMan.Cli;

public static class ArgumentParser
{
    /// <summary>Looks for "--name value" anywhere in args and returns the value.</summary>
    public static (string? Value, bool Found) GetOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return (args[i + 1], true);
            }
        }

        return (null, false);
    }
}
