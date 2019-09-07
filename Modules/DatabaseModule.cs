using System.Collections.Specialized;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modules{

    public class DatabaseModule : ModuleBase<SocketCommandContext>{
        private DatabaseService database;
    
        public DatabaseModule(DatabaseService dbs){
            this.database = dbs;
        }

        [Command("Execute")]
        public async Task Execute([Remainder]string CMD){
            ulong gid = Context.Guild.Id;
            Check(gid);
            GuildDatabase db = database.GetDatabase(gid);
            var command = db.Connection.CreateCommand();
            command.CommandText = CMD;

            string fin = "";

            using(var reader = command.ExecuteReader()){
                while (reader.Read()){
                    ulong id = (ulong)reader["Id"];
                }
            }
        }

        private void Check(ulong gid){
            if(!database.Exist(gid))
                database.AddDatabase(gid);
        }

    }

}