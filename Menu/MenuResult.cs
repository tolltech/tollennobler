using System.Collections.Generic;

namespace Tolltech.TollEnnobler.Menu
{
    public class MenuResult
    {
        public MenuCommandType MenuCommandType { get; set; }
        public Dictionary<string, string[]> CommandParameters { get; set; }
    }
}