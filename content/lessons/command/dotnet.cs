var editor = new TextEditor();
var history = new CommandHistory();

history.Run(new AppendCommand(editor, "سلام"));
history.Run(new AppendCommand(editor, " دنیا"));
Console.WriteLine(editor.Text); // سلام دنیا

history.Undo();
Console.WriteLine(editor.Text); // سلام

history.Redo();
Console.WriteLine(editor.Text); // سلام دنیا

class TextEditor
{
    public string Text { get; set; } = "";
}

interface ICommand
{
    void Execute();
    void Undo();
}

class AppendCommand(TextEditor editor, string text) : ICommand
{
    public void Execute() => editor.Text += text;
    public void Undo() => editor.Text = editor.Text[..^text.Length];
}

class CommandHistory
{
    private readonly Stack<ICommand> _done = new();
    private readonly Stack<ICommand> _undone = new();

    public void Run(ICommand command)
    {
        command.Execute();
        _done.Push(command);
        _undone.Clear();
    }

    public void Undo()
    {
        if (!_done.TryPop(out var command)) return;
        command.Undo();
        _undone.Push(command);
    }

    public void Redo()
    {
        if (!_undone.TryPop(out var command)) return;
        command.Execute();
        _done.Push(command);
    }
}
