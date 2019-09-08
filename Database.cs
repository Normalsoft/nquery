using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

public class DatabaseService : IDisposable
{
  public Dictionary<ulong, GuildDatabase> Databases = new Dictionary<ulong, GuildDatabase>();

  private static DatabaseService instance = new DatabaseService();

  public static DatabaseService Instance { get => instance; }

  public void AddDatabase(ulong gid)
  {
    if (!Directory.Exists("Databases")) Directory.CreateDirectory("Databases");

    GuildDatabase gdb = new GuildDatabase();
    if (!gdb.Start($"Databases/{gid}.db"))
      throw new Exception("Failed to load/create database.");

    Databases.Add(gid, gdb);
  }

  public void DisposeDatabase(ulong gid)
  {
    if (!Databases.ContainsKey(gid))
      throw new Exception("Requested guild has no database.");

    Databases[gid]?.Dispose();
    Databases.Remove(gid);
  }

  public bool Exist(ulong gid) => Databases.ContainsKey(gid);

  public GuildDatabase GetDatabase(ulong gid)
  {
    if (!Databases.ContainsKey(gid))
      throw new Exception("Requested guild has no database.");
    return Databases[gid];
  }

  public void Dispose()
  {
    foreach (var gdb in Databases)
    {
      gdb.Value.Dispose();
    }
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

  public void Dispose()
  {
    Connection.Dispose();
  }
}