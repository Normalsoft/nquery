using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using static System.Linq.Enumerable;
using MarkdownTable;

public class DatabaseService : IDisposable
{
  public Dictionary<string, GuildDatabase> Databases = new Dictionary<string, GuildDatabase>();

  private static DatabaseService instance = new DatabaseService();

  public static DatabaseService Instance { get => instance; }

  public void AddDatabase(string gid)
  {
    if (Exists(gid)) return;
    if (!Directory.Exists(".db")) Directory.CreateDirectory(".db");

    GuildDatabase gdb = new GuildDatabase();
    if (!gdb.Start($".db/{gid}.db"))
      throw new Exception("Failed to load/create database.");

    Databases.Add(gid, gdb);
  }

  public void DisposeDatabase(string gid)
  {
    if (!Databases.ContainsKey(gid))
      throw new Exception("Requested guild has no database.");

    Databases[gid]?.Dispose();
    Databases.Remove(gid);
  }

  public bool Exists(string gid) => Databases.ContainsKey(gid);

  public GuildDatabase GetDatabase(string gid)
  {
    if (!Databases.ContainsKey(gid))
      throw new Exception("Requested guild has no database.");
    return Databases[gid];
  }

  public GuildDatabase GetOrInitDatabase(string gid)
  {
    if (!Exists(gid)) AddDatabase(gid);
    return GetDatabase(gid);
  }

  public void Dispose()
  {
    foreach (var gdb in Databases)
      gdb.Value.Dispose();
  }
}

public class GuildDatabase : IDisposable
{
  public SQLiteConnection Connection;

  public bool Start(string FileLoc)
  {
    if (!File.Exists(FileLoc)) SQLiteConnection.CreateFile(FileLoc);

    Connection = new SQLiteConnection("Data Source=" + FileLoc + ";Version=3");
    Connection.Open();

    return Connection.State == ConnectionState.Open;
  }

  public bool TableExists(String tableName)
  {
    SQLiteCommand cmd = Connection.CreateCommand();
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
    cmd.Parameters.AddWithValue("@name", tableName);

    return (cmd.ExecuteScalar() != null);
  }

  public SQLiteCommand CreateCommand(string cmd)
  {
    var command = Connection.CreateCommand();
    command.CommandText = cmd;
    return command;
  }

  public string QueryToMarkdownTable(SQLiteCommand cmd)
  {
    using (var reader = cmd.ExecuteReader())
    {
      if (reader.FieldCount < 1) return "";
      var ct = new MarkdownTableBuilder().WithHeader(
        Range(0, reader.FieldCount)
          .Select(x => reader.GetName(x)).ToArray()
      );

      while (reader.Read())
        ct.WithRow(
          Range(0, reader.FieldCount)
            .Select(x => reader.GetValue(x).ToString()).ToArray()
        );
      return ct.ToString();
    }
  }

  public string QueryToCsv(SQLiteCommand cmd, bool includeHeaders = false)
  {
    using (var reader = cmd.ExecuteReader())
    {
      if (reader.FieldCount < 1) return "";
      string text = "";
      if (includeHeaders)
        String.Join(",",
          Range(0, reader.FieldCount).Select(x => reader.GetName(x)).ToArray());

      while (reader.Read())
        text += String.Join(",", Range(0, reader.FieldCount)
          .Select(x => reader.GetValue(x).ToString()).ToArray()) + "\n";
      return text;
    }
  }

  public Object QueryToScalar(SQLiteCommand cmd) => cmd.ExecuteScalar();

  public void Dispose() => Connection.Dispose();

  public enum QueryOutputType
  {
    MarkdownTable, Scalar, Csv
  }

  public static QueryOutputType QueryTypeShorthand(string sh)
  {
    switch (sh)
    {
      case "t":
      case "table":
      case "tab":
      case "md":
        return QueryOutputType.MarkdownTable;
      case "s":
      case "scalar":
      case "sc":
        return QueryOutputType.Scalar;
      case "c":
      case "csv":
        return QueryOutputType.Csv;
      default:
        throw new UserCommandException("no such query output type");
    }
  }

  public string Query(QueryOutputType type, SQLiteCommand cmd)
  {
    switch (type)
    {
      case QueryOutputType.MarkdownTable: return QueryToMarkdownTable(cmd);
      case QueryOutputType.Scalar: return QueryToScalar(cmd)?.ToString() ?? "";
      case QueryOutputType.Csv: return QueryToCsv(cmd);
      default: throw new Exception("invalid QueryOutputType");
    }
  }

  public void StoreQuery(string name, string opts, string rest)
  {
    if (!TableExists("nquery_queries"))
    {
      CreateCommand(
        @"CREATE TABLE nquery_queries (
          id INTEGER PRIMARY KEY,
          name TEXT NOT NULL UNIQUE,
          body TEXT NOT NULL,
          opts TEXT NOT NULL)"
      ).ExecuteScalar();
    }
    var cmd = CreateCommand(@"INSERT OR REPLACE INTO nquery_queries 
      (name, body, opts) VALUES (@name, @body, @opts)");
    cmd.Parameters.AddWithValue("@name", name);
    cmd.Parameters.AddWithValue("@body", rest);
    cmd.Parameters.AddWithValue("@opts", opts);
    cmd.ExecuteScalar();
  }

  public Dictionary<string, string> RecallOptions(string name)
  {
    var cmd = CreateCommand(@"SELECT opts FROM nquery_queries WHERE name = @name");
    cmd.Parameters.AddWithValue("@name", name);
    return MiscUtil.ExtractOpts(cmd.ExecuteScalar()?.ToString() ?? "");
  }

  public string RecallQuery(QueryOutputType type, string name, string[] args)
  {
    var cmd = CreateCommand(@"SELECT body FROM nquery_queries WHERE name = @name");
    cmd.Parameters.AddWithValue("@name", name);
    var sql = cmd.ExecuteScalar()?.ToString();
    if (sql == null) throw new UserCommandException($"no such stored query: {name}");
    var cmd2 = CreateCommand(sql);
    cmd2.Parameters.AddWithValue("@0", String.Join(" ", args));
    for (var i = 0; i < args.Length; i++)
    {
      cmd2.Parameters.AddWithValue($"@{i + 1}", args[i]);
    }
    return Query(type, cmd2);
  }

  public void DeleteStoredQuery(string name)
  {
    var cmd = CreateCommand(@"DELETE FROM nquery_queries WHERE name = @name");
    cmd.Parameters.AddWithValue("@name", name);
    if (cmd.ExecuteScalar() == null)
      throw new UserCommandException($"no such stored query: {name}");
  }

  public string[] ListQueryNames()
  {
    var cmd = CreateCommand(@"SELECT name FROM nquery_queries");
    string[] names = new string[] { };
    using (var reader = cmd.ExecuteReader())
    {
      while (reader.Read()) names = names.Append(reader.GetValue(0).ToString()).ToArray();
    }
    return names;
  }
}

public class UserCommandException : Exception
{
  public UserCommandException(string message) : base(message) { }
}