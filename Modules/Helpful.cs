using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modules
{
    public class Helpful : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;
        private DiscordSocketClient _client;
        private Bot bot;

        public Helpful(CommandService service, DiscordSocketClient client, Bot _bot)
        {
            _service = service;
            _client = client;
            bot = _bot;
        }

        [Command("Benchmark")]
        public async Task Benchmark()
        {
            var time = DateTimeOffset.Now.Subtract(Context.Message.CreatedAt);
            await Context.Channel.SendMessageAsync($"From message to now it took {time.Seconds}:{time.Milliseconds} Sec:Milsec");
        }

        #region Help

        [Command("Help")]
        [Summary("Get list of modules with their summary.")]
        public async Task HelpAsync(string reqmodule = null)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var builder = new EmbedBuilder()
            {
                Color = Discord.Color.Green,
                Description = $"Showing info for {Context.Guild.Name}.\n"
            };

            if (reqmodule == null)
            {
                builder.Description += "here's all the modules currently loaded.";
                foreach (var module in _service.Modules)
                {
                    if (module.Name == "Secrets") continue;

                    string Sum = "This module has no summary.";
                    if (module.Summary != null)
                        Sum = module.Summary;

                    builder.AddField(module.Name, Sum, true);
                }
            }
            else
            {
                //So they want to see a req module
                var module = _service.Modules.FirstOrDefault(x => String.Equals(x.Name, reqmodule, StringComparison.CurrentCultureIgnoreCase));
                if (module != null)
                {
                    builder.Description += $"\nShowing all commands for {module.Name}.";
                    foreach (var cmd in module.Commands)
                    {
                        if (module.Name == "Secrets") continue;

                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                        {
                            string Sum = "This command has no summary.";

                            if (cmd.Summary != null)
                                Sum = cmd.Summary;

                            builder.AddField(x =>
                            {
                                x.Name = cmd.Name;
                                x.Value = Sum;
                                x.IsInline = true;
                            });
                        }
                    }
                }
            }

            await dmChannel.SendMessageAsync("", false, builder.Build());
        }

        [Command("CMD")]
        [Summary("Gives you more info on a command.")]
        public async Task CMDAsync(string cmd)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var builder = new EmbedBuilder()
            {
                Color = Discord.Color.Green,
                Description = $"Showing info for {Context.Guild.Name}.\n"
            };

            var command = _service.Commands.FirstOrDefault(x => String.Equals(x.Name, cmd, StringComparison.CurrentCultureIgnoreCase));

            if (command != null)
            {
                builder.Description = $"Command: {command.Name}";
                builder.AddField("Summary", (command.Summary ?? "This command has no summary."));
                builder.AddField("Remarks", command.Remarks ?? "This command has no remarks.");
                if (command.HasVarArgs || !command.Parameters.Equals(null))
                {
                    string final = $"{bot.Prefix}{command.Name} ";

                    foreach (var parameter in command.Parameters)
                    {
                        string psum = "";
                        if (parameter.Summary != null)
                            psum = $" | Summary: {parameter.Summary}";

                        final += $"[Name: {parameter.Name}{psum}] ";
                    }

                    builder.AddField("Sample", final);
                }

                await dmChannel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                await ReplyAsync(
                    "Sorry, couldn't find the command, you either need to check your spelling or its not there.");
                return;
            }
        }

        #endregion

    }
}
