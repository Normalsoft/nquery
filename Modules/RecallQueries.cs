using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Data.Common;
using System;
using System.Collections.Generic;

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
    public async Task StoreQuery(string opts, string name, [Remainder]string rest)
    {
      Dictionary<string, string> options;
      if (opts.StartsWith("?"))
        options = MiscUtil.ExtractOpts(opts);
      else
      {
        rest = name + " " + rest;
        name = opts;
        options = new Dictionary<string, string>();
      }
      GuildDatabase db = dsv.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      db.StoreQuery(name, MiscUtil.DictToOpts(options), rest);
      await Context.Message.AddReactionAsync(new Emoji("🆗"));
    }

    [Command("Recall")]
    [Alias("qr")]
    public async Task RecallQuery(string name, params string[] args)
    {
      GuildDatabase db = dsv.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      Dictionary<string, string> options;
      if (name.StartsWith("?"))
      {
        options = MiscUtil.ExtractOpts(name);
        name = args[0];
        Array.Copy(args, 1, args, 0, args.Length - 1);
      }
      else
        options = db.RecallOptions(name);

      if (options.ContainsKey("t")) options.Add("type", options["t"]);
      if (!options.ContainsKey("type")) options.Add("type", "table");

      try
      {
        var tt = db.RecallQuery(GuildDatabase.QueryTypeShorthand(options["type"]), name, args);
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

    [Command("List")]
    [Alias("ql")]
    public async Task ListQueries()
    {
      GuildDatabase db = dsv.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      var names = db.ListQueryNames();
      if (names.Length > 0) await ReplyAsync(String.Join(", ", names));
      else await Context.Message.AddReactionAsync(new Emoji("⛔"));
    }

    [Command("Delete")]
    [Alias("qd")]
    public async Task DeleteQuery(string name)
    {
      GuildDatabase db = dsv.GetOrInitDatabase($"dc-{Context.Guild.Id}");
      try
      {
        db.DeleteStoredQuery(name);
        await Context.Message.AddReactionAsync(new Emoji("🆗"));
      }
      catch (Exception e) when (e is DbException || e is UserCommandException)
      {
        await ReplyAsync(":exclamation: " + e.Message.Replace("SQL logic error\n", "").Trim());
      }
    }

  }
}