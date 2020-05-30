using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockstarGamesLibrary
{
    public class RockstarGamesLibraryClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => RockstarGames.Icon;

        public override bool IsInstalled => RockstarGames.IsInstalled;

        public override void Open()
        {
            RockstarGames.StartClient();
        }
    }
}