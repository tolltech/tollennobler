using System.Collections.Generic;

namespace Tolltech.TollEnnobler.Menu
{
    public class MenuArgs
    {
        public static readonly MenuArgs Unknown = new MenuArgs
        {
            CommandParameters = new Dictionary<string, string[]>(),
            CommandType = MenuCommandType.Unknown
        };

        public MenuArgs()
        {
            CommandParameters = new Dictionary<string, string[]>();
        }

        public MenuArgs(MenuCommandType commandType, Dictionary<string, string[]> commandParameters = null)
        {
            CommandParameters = commandParameters ?? new Dictionary<string, string[]>();
            CommandType = commandType;
        }

        public MenuCommandType CommandType { get; private set; }
        public IReadOnlyDictionary<string, string[]> CommandParameters { get; private set; }
    }
}