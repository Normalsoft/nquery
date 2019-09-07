using System;
using System.Threading.Tasks;

namespace nquery
{
    class Program
    {
        Bot botInstance;

        [STAThread]
        private static void Main(string[] args) => new Program().StartAsync(args).GetAwaiter().GetResult();

        private async Task StartAsync(string[] args){
            AppDomain.CurrentDomain.ProcessExit += Exit;
            
            botInstance = new Bot();

            await botInstance.Start();
        }

        private void Exit(object sender, EventArgs e){
            botInstance.Dispose();
        }
    }
}
