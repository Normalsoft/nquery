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

    [Command("SQL")]
    public async Task Execute([Remainder]string cmd)
    {
      ulong gid = Context.Guild.Id;
      if (!database.Exists($"dc-{gid}"))
        await Context.Message.AddReactionAsync(new Emoji("📡"));
      GuildDatabase db = database.GetOrInitDatabase($"dc-{gid}");
      var command = db.Connection.CreateCommand();
      command.CommandText = cmd;

      try
      {
        using (var reader = command.ExecuteReader())
        {
          await Context.Message.AddReactionAsync(new Emoji("🆗"));
          if (reader.FieldCount < 1) return;

          var ct = new MarkdownTableBuilder().WithHeader(
            Range(0, reader.FieldCount)
              .Select(x => reader.GetName(x)).ToArray()
          );

          while (reader.Read())
          {
            ct.WithRow(
              Range(0, reader.FieldCount)
                .Select(x => reader.GetValue(x).ToString()).ToArray()
            );
          }
          await ReplyAsync("```\n" + ct.ToString() + "\n```");
        }
      }
      catch (DbException e)
      {
        await ReplyAsync(":exclamation: " + e.Message.Replace("SQL logic error\n", "").Trim());
      }
    }

    [Command("DC")]
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

    [Command("Co")]
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

    [Command("DBN")]
    public async Task DBName()
    {
      string dbid = $"dc-{Context.Guild.Id}";
      await ReplyAsync(
        $"Database name is `{dbid}`.\n"
        + $"Database is {(database.Exists(dbid) ? "connected" : "disconnected")}.");
    }
  }

}