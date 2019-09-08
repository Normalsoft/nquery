using System.Collections.Specialized;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.Common;
using static System.Linq.Enumerable;
using MarkdownTable;

namespace Modules
{

  public class DatabaseModule : ModuleBase<SocketCommandContext>
  {
    private DatabaseService database;

    public DatabaseModule(DatabaseService dbs)
    {
      this.database = dbs;
    }

    [Command("Table")]
    [Alias("tq")]
    public async Task TableQuery([Remainder]string cmd)
    {
      if (!database.Exists($"dc-{Context.Guild.Id}"))
        await Context.Message.AddReactionAsync(new Emoji("📡"));
      GuildDatabase db = database.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      var command = db.CreateCommand(cmd);

      try
      {
        var tt = db.QueryToTextTable(command);
        await Context.Message.AddReactionAsync(new Emoji("🆗"));
        if (tt.Length > 0)
          await ReplyAsync("```\n" + tt + "\n```");
      }
      catch (DbException e)
      {
        await ReplyAsync(":exclamation: " + e.Message.Replace("SQL logic error\n", "").Trim());
      }
    }

    [Command("Scalar")]
    [Alias("sq")]
    public async Task ScalarQuery([Remainder] string cmd)
    {
      if (!database.Exists($"dc-{Context.Guild.Id}"))
        await Context.Message.AddReactionAsync(new Emoji("📡"));
      GuildDatabase db = database.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      var command = db.CreateCommand(cmd);

      try
      {
        var tt = db.QueryToValue(command).ToString();
        await Context.Message.AddReactionAsync(new Emoji("🆗"));
        await ReplyAsync(tt);
      }
      catch (DbException e)
      {
        await ReplyAsync(":exclamation: " + e.Message.Replace("SQL logic error\n", "").Trim());
      }
    }

    [Command("Disconnect")]
    [Alias("dc")]
    public async Task Disconnect()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      if (!database.Exists(dbid))
      {
        await Context.Message.AddReactionAsync(new Emoji("❌"));
        return;
      }
      database.DisposeDatabase(dbid);
      await Context.Message.AddReactionAsync(new Emoji("🗑"));
    }

    [Command("Connect")]
    [Alias("co")]
    public async Task Connect()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      if (database.Exists(dbid))
        await Context.Message.AddReactionAsync(new Emoji("✅"));
      else
      {
        database.AddDatabase(dbid);
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
        + $"Database is {(database.Exists(dbid) ? "connected" : "disconnected")}.");
    }


  }

}