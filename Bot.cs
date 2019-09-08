using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using System.IO;
using dotenv.net;

public class Bot : IDisposable
{
  private CommandService Commands;
  private DiscordSocketClient Client;
  private IServiceProvider Services;

  public async Task Start()
  {
    DotEnv.Config();
    if (!File.Exists("config.toml"))
    {
      Console.WriteLine("config.toml doesnt exist, please create it.");
      return;
    }

    Client = new DiscordSocketClient();
    Commands = new CommandService();

    ServiceCollection ServiceCollection = new ServiceCollection();
    ServiceCollection.AddSingleton(Client);
    ServiceCollection.AddSingleton(Commands);
    ServiceCollection.AddSingleton(this);
    ServiceCollection.AddSingleton(DatabaseService.Instance);
    ServiceCollection.AddSingleton(ConfigService.Instance);
    Services = ServiceCollection.BuildServiceProvider();

    Client.MessageReceived += HandleCommandAsync;
    await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

    await Client.LoginAsync(
      Discord.TokenType.Bot,
      Environment.GetEnvironmentVariable("TOKEN"));
    await Client.StartAsync();
    await Client.SetStatusAsync(Discord.UserStatus.Online);

    await Task.Delay(-1);
  }

  private async Task HandleCommandAsync(SocketMessage messageParam)
  {
    int argPos = 0;
    SocketUserMessage message = (SocketUserMessage)messageParam;

    if (message == null) return;

    SocketGuildChannel channel = (SocketGuildChannel)message.Channel;
    SocketCommandContext context = new SocketCommandContext(Client, message);

    if (!(message.HasStringPrefix(ConfigService.config["prefix"], ref argPos) ||
        message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;

    var result = await Commands.ExecuteAsync(context, argPos, Services);
    if (result.IsSuccess) return;
    new Thread(async () =>
    {
      Discord.Rest.RestUserMessage tmg;
      switch (result.Error)
      {
        case CommandError.UnknownCommand:
          await context.Message.AddReactionAsync(new Emoji("❓"));
          break;

        case CommandError.BadArgCount:
          tmg = await context.Channel.SendMessageAsync("Bad number of arguments.");
          await Task.Delay(5000);
          await tmg?.DeleteAsync();
          break;

        case CommandError.UnmetPrecondition:
          tmg = await context.Channel.SendMessageAsync(result.ErrorReason);
          await Task.Delay(5000);
          await tmg?.DeleteAsync();
          break;

        default:
          var embed = new EmbedBuilder()
                  .WithTitle("Whoa!")
                  .WithColor(Color.Red)
                  .WithDescription("Something went wrong! Please message the developer with this error report.\n\nThank you!")
                  .AddField("Error Report", "```\n" + result.ToString() + "\n```", true)
                  .Build();
          await context.Channel.SendMessageAsync("", false, embed);
          break;

      }
    }).Start();
  }

  public void Dispose()
  {
    Client.SetStatusAsync(Discord.UserStatus.Offline);
    Client.LogoutAsync();
    Client.Dispose();
  }
}