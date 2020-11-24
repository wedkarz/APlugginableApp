public interface IPlugin
{
    string Description { get; }
    string Execute(string input);
}
