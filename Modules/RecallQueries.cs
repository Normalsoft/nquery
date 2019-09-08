using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Data.Common;

namespace Modules
{
  public class RecallQueriesModule : ModuleBase<SocketCommandContext>
  {
    private DatabaseService dsv;

    public RecallQueriesModule(DatabaseService dbs)
    {
      this.dsv = dbs;
    }

    [Command("Store")]
    [Alias("qs")]
    public async Task StoreQuery(string name, string opts, [Remainder]string rest)
    {
      await ReplyAsync("WIP");
    }

    [Command("Recall")]
    [Alias("qr")]
    public async Task RecallQuery(string name, string opts, [Remainder]string rest)
    {
      await ReplyAsync("WIP");
    }

  }
}