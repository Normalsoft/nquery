using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Data.Common;
using System;

namespace Modules
{

  public class RawQueriesModule : ModuleBase<SocketCommandContext>
  {
    private DatabaseService dsv;

    public RawQueriesModule(DatabaseService dbs)
    {
      this.dsv = dbs;
    }

    [Command("Query")]
    [Alias("gq", "q")]
    public async Task GenericQuery(string opts, [Remainder] string cmd)
    {
      var options = MiscUtil.ExtractOpts("");
      if (opts.StartsWith("?"))
        options = MiscUtil.ExtractOpts(opts);
      else
        cmd = opts + " " + cmd;
      if (options.ContainsKey("t")) options.Add("type", options["t"]);
      if (!options.ContainsKey("type")) options.Add("type", "table");

      if (!dsv.Exists($"dc-{Context.Guild.Id}"))
        await Context.Message.AddReactionAsync(new Emoji("📡"));
      GuildDatabase db = dsv.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      var command = db.CreateCommand(cmd);

      try
      {
        var tt = db.Query(GuildDatabase.QueryTypeShorthand(options["type"]), command);
        await Context.Message.AddReactionAsync(new Emoji("🆗"));
        if (tt.Length > 0)
          if (options.ContainsKey("nowrap")) await ReplyAsync(tt);
          else await ReplyAsync($"```\n{tt}\n```");
        else await Context.Message.AddReactionAsync(new Emoji("🤔"));
      }
      catch (Exception e) when (e is DbException || e is UserCommandException)
      {
        await ReplyAsync(":exclamation: " + e.Message.Replace("SQL logic error\n", "").Trim());
      }

    }

    [Command("Table")]
    [Alias("tq")]
    public async Task TableQuery([Remainder]string cmd)
    {
      await GenericQuery("?type=table", cmd);
    }

    [Command("Scalar")]
    [Alias("sq")]
    public async Task ScalarQuery([Remainder]string cmd)
    {
      await GenericQuery("?type=scalar&nowrap", cmd);
    }

    [Command("CSV")]
    [Alias("cq")]
    public async Task CsvQuery([Remainder]string cmd)
    {
      await GenericQuery("?type=csv", cmd);
    }

    [Command("Disconnect")]
    [Alias("dc")]
    public async Task Disconnect()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      if (!dsv.Exists(dbid))
      {
        await Context.Message.AddReactionAsync(new Emoji("❌"));
        return;
      }
      dsv.DisposeDatabase(dbid);
      await Context.Message.AddReactionAsync(new Emoji("🗑"));
    }

    [Command("Connect")]
    [Alias("co")]
    public async Task Connect()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      if (dsv.Exists(dbid))
        await Context.Message.AddReactionAsync(new Emoji("✅"));
      else
      {
        dsv.AddDatabase(dbid);
        await Context.Message.AddReactionAsync(new Emoji("📡"));
      }
    }

    [Command("DBName")]
    [Alias("dbn")]
    public async Task DbName()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      await ReplyAsync(
        $"Database name is `{dbid}`.\n"
        + $"Database is {(dsv.Exists(dbid) ? "connected" : "disconnected")}.");
    }
  }
}