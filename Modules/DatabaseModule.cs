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
      if (Check(gid)) await ReplyAsync(":ok: Initialized a connection for your server.");
      GuildDatabase db = database.GetDatabase(gid);
      var command = db.Connection.CreateCommand();
      command.CommandText = cmd;

      try
      {
        using (var reader = command.ExecuteReader())
        {
          await Context.Message.AddReactionAsync(new Emoji("🆗"));

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

    private bool Check(ulong gid)
    {
      if (!database.Exist(gid))
      {
        database.AddDatabase(gid);
        return true;
      }
      return false;
    }

  }

}