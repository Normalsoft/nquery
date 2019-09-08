using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

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

  public void Dispose()
  {
    Connection.Dispose();
  }
}