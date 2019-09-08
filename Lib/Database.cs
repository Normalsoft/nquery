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
    cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='@name';";
    cmd.Parameters.Add("@name", DbType.String).Value = tableName;

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
}

public class UserCommandException : Exception
{
  public UserCommandException(string message) : base(message) { }
}