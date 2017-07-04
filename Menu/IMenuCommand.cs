namespace Tolltech.TollEnnobler.Menu
{
    public interface IMenuCommand
    {
        MenuCommandType CommandType { get; }
        void Run();
    }
}