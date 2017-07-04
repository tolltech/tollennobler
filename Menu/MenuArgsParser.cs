using System;
using System.Linq;
using log4net;

namespace Tolltech.TollEnnobler.Menu
{
    public class MenuArgsParser : IMenuArgsParser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IMenuArgsParser));

        public MenuArgs Parse(string args)
        {
            var currentArgs = args?.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (currentArgs == null || currentArgs.Length == 0)
            {
                log.ToError($"Cant parse command from args {args}");
                return MenuArgs.Unknown;
            }

            MenuCommandType commandType;
            if (!Enum.TryParse(currentArgs[0], true, out commandType) ||
                !Enum.IsDefined(typeof(MenuCommandType), commandType))
            {
                log.ToError($"Cant parse command from args {currentArgs[0]}");
                return MenuArgs.Unknown;
            }

            var commandArgsStr = string.Join(" ", currentArgs.Skip(1));
            commandArgsStr.
        }
    }
}