/*
using System.Threading.Tasks;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;

namespace brainKiller.Utilities
{
    public class BotList
    {
        private static BotList _instance;

        // private AuthDiscordBotListApi Api { get; init; }
        //private DiscordBotListApi Api = new DiscordBotListApi();
        private AuthDiscordBotListApi Api = new AuthDiscordBotListApi(808888674900508723,
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjgwODg4ODY3NDkwMDUwODcyMyIsImJvdCI6dHJ1ZSwiaWF0IjoxNjE2NDc2ODU4fQ.kHioscliiyc_ibfJcK7nGrTDUKTCFEOcMnOom76TZ84");

        private BotList()
        {
        }

        public IDblSelfBot ThisBot { get; private set; }

        public static async Task<BotList> Instantiate(ulong botId, string topGgToken)
        {
            if (_instance != null) return _instance;
            _instance = new BotList {Api = new AuthDiscordBotListApi(botId, topGgToken)};
            _instance.ThisBot = await _instance.Api.GetMeAsync();
            return _instance;
        }
    }
}
*/

