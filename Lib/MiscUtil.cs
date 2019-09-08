using System.Collections.Generic;
using System.Text.RegularExpressions;

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
      if (key.Length == 2) opts.Add(key[0], key[1]);
      else opts.Add(key[0], "true");
    }
    return opts;
  }
}