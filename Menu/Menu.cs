using System;
using System.Linq;
using log4net;

namespace Tolltech.TollEnnobler.Menu
{
    public class Menu : IMenu
    {
        private readonly IMenuCommand[] menuCommands;
        private readonly IMenuArgsParser menuArgsParser;
        private static readonly ILog log = LogManager.GetLogger(typeof(Menu));

        public Menu(IMenuCommand[] menuCommands, IMenuArgsParser menuArgsParser)
        {
            this.menuCommands = menuCommands;
            this.menuArgsParser = menuArgsParser;
        }

        public void Run(string args)
        {
            var batMode = !string.IsNullOrWhiteSpace(args);

            var menuCommandType = MenuCommandType.Unknown;
            do
            {
                var currentArgsStr = batMode ? args : Console.ReadLine();

                var menuArgs = menuArgsParser.Parse(currentArgsStr);

                foreach (var menuCommand in menuCommands)
                {
                    if (menuCommand.CommandType == menuArgs.CommandType)
                    {
                        menuCommand.Run(menuArgs.CommandParameters);
                        break;
                    }
                }

            } while (!batMode && menuCommandType != MenuCommandType.Exit && menuCommandType != MenuCommandType.Unknown);
        }
    }
}