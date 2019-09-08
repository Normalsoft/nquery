using System;
using System.IO;
using Tommy;

public class ConfigService
{
  public static TomlTable config;

  public static ConfigService instance = new ConfigService();

  public static ConfigService Instance { get => instance; }

  ConfigService() => this.ReloadConfig();

  public void ReloadConfig()
  {
    using (StreamReader reader = new StreamReader(File.OpenRead("config.toml")))
      config = TOML.Parse(reader);
  }
}