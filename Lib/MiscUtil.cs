using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class MiscUtil
{
  static Regex rx = new Regex(@"^(\w+?)(\?.+?)(\s.*)?$", RegexOptions.Compiled);
  public static string TransformCommand(string cmd)
  {
    if (rx.IsMatch(cmd))
    {
      Match m = rx.Matches(cmd)[0];
      return $"{m.Groups[1]} \"{m.Groups[2]}\"{m.Groups[3]}";
    }
    else return cmd;
  }

  public static Dictionary<string, string> ExtractOpts(string str)
  {
    Dictionary<string, string> opts = new Dictionary<string, string>();
    str = str.TrimStart('?');
    foreach (var pair in str.Split('&'))
    {
      var key = pair.Split('=', 2);
      if (key.Length == 2 && key[0].Length > 0) opts.Add(key[0], key[1]);
      else if (key[0].Length > 0) opts.Add(key[0], "true");
    }
    return opts;
  }

  public static Dictionary<string, string> ExtractOrNot(string opts, ref string cmd)
  {
    var options = MiscUtil.ExtractOpts("");
    if (opts.StartsWith("?"))
      options = MiscUtil.ExtractOpts(opts);
    else
      cmd = (opts + " " + cmd).Trim();
    return options;
  }

  public static string DictToOpts(Dictionary<string, string> options) =>
    String.Join("&", options.Keys.ToArray().Select(x => $"{x}={options[x]}"));

}