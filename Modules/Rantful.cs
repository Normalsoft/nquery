using Rant;
using Discord;
using Discord.Commands;
using System.Data.Common;
using System;
using System.Threading.Tasks;

namespace Modules
{
  public class RantModule : ModuleBase<SocketCommandContext>
  {
    RantEngine rant = new RantEngine();
    public RantModule()
    {
      rant.LoadPackage("./Rantionary.rantpkg");
    }

    [Command("Rant")]
    public async Task MakeRant([Remainder] string cmd)
    {
      var pgm = RantProgram.CompileString(cmd);
      await ReplyAsync(rant.Do(pgm).Main);
    }
  }
}