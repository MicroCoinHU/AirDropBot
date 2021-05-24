using Discord;
using Discord.WebSocket;
using System.Linq;
using MicroCoin.API.Api;
using MicroCoin.API.Model;
using MicroCoin.Cryptography;
using MicroCoin.Types;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RewardBot
{
    public class AirDrop
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
    }

    class Program
    {
        // Configuration section
        private const string DiscordToken = "";
        private const string botAccount = "5555-98";
        // Private key
        private const string pKey = "";
        private const string payload = "AirDrop";
        private const string channel = "airdrop";        
        private const string ErrorText = "Hibás számlaszám";
        private const string dbFile = "rdb.db";

        private static string dbName = "";

        private static DiscordSocketClient _client;
        static async Task Main(string[] args)
        {
            dbName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RewardBot-Data");
            if (!Directory.Exists(dbName))
            {
                Directory.CreateDirectory(dbName);
            }
            dbName = Path.Combine(dbName, dbFile);
            await StartClient();
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            await Task.Delay(-1);
        }
        private static int flag = 0;
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    if (flag == 0)
                    {
                        var json = new WebClient().DownloadString("https://api.coingecko.com/api/v3/coins/microcoin");
                        var jo = JObject.Parse(json);
                        var price = jo.Value<JObject>("market_data").Value<JObject>("current_price").Value<decimal>("huf");
                        _client.SetGameAsync(string.Format("{0:N} Ft @ FinexBox", price), null, ActivityType.Watching);
                    }
                    else if (flag == 1)
                    {
                        var api = new AccountApi();
                        var account = api.GetAccount("5555");
                        _client.SetGameAsync(string.Format("{0:N} MCC @ {1}", account.Balance, botAccount), null, ActivityType.Watching);
                    }
                    else if (flag == 2)
                    {
                        var json = new WebClient().DownloadString("https://blockexplorer.microcoin.hu/api/blocks");
                        var jo = JObject.Parse(json);
                        _client.SetGameAsync(string.Format("Utolsó blokk: {0}", UnixTimeStampToDateTime(jo.Value<JArray>("blocks").Value<JObject>(0).Value<int>("timestamp")).ToShortTimeString()), null, ActivityType.Watching);
                    }
                }
                catch
                {

                }
            }).Start();
            flag++;
            if (flag == 3) flag = 0;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static async Task StartClient()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.LoggedIn += _client_LoggedIn;
            _client.MessageReceived += MessageReceivedAsync;
            _client.Disconnected += _client_Disconnected;
            _client.JoinedGuild += _client_JoinedGuild;
            await _client.LoginAsync(TokenType.Bot, DiscordToken);
            await _client.StartAsync();
        }

        private static async Task _client_JoinedGuild(SocketGuild arg)
        {
        }

        private static async Task _client_LoggedIn()
        {
        }

        private static async Task _client_Disconnected(Exception arg)
        {
            _client.Dispose();
            _client = null;
            await StartClient();
        }

        private static bool CheckAccount(string account)
        {
            if (!account.Contains("-"))
            {
                return false;
            }
            var acc = account.Split("-");
            if (!int.TryParse(acc[0], out int acn))
            {
                return false;
            }
            if (!int.TryParse(acc[1], out int chk))
            {
                return false;
            }
            var checksum = ((acn * 101) % 89) + 10;
            if (checksum != chk)
            {
                return false;
            }
            return true;
        }

        private static async Task MessageReceivedAsync(SocketMessage message)
        {
            try
            {
                if (message.Author.IsBot || message.Author.Id == _client.CurrentUser.Id) return;
                if (message.Content.Length < 3) return;

                using var db = new LiteDB.LiteDatabase(dbName);

                var userId = message.Author.Id;

                if (message.Channel.Name.Contains(channel))
                {
                    var airdrop = db.GetCollection<AirDrop>();
                    var ex = airdrop.FindOne(p => p.UserId == userId);
                    if (ex != null)
                    {
                        await message.Channel.SendMessageAsync("Te már igényeltél!");
                        return;
                    }
                    var account = message.Content;
                    if (!CheckAccount(account))
                    {
                        await message.Channel.SendMessageAsync(ErrorText);
                        return;
                    }
                    var amountToSend = new Random().Next(1, 100);
                    SendCoins(account, amountToSend);
                    
                    await message.Channel.SendMessageAsync($"Küldtem neked {amountToSend} MicroCoint");

                    airdrop.Insert(new AirDrop
                    {
                        UserId = userId,
                        Account = account,
                        Amount = amountToSend,
                        DateTime = DateTime.Now
                    });
                    return;
                }
            }
            catch { }
        }

        private static void SendCoins(string account, int amountToSend)
        {
            var api = new TransactionApi();
            using CryptoService service = new CryptoService();
            var myKey = ECKeyPair.Import(pKey);
            var tr = api.StartTransaction(new TransactionRequest(amountToSend, 0.0001M, payload, botAccount, account));
            var signature = service.GenerateSignature(tr.Hash, myKey);
            tr.Signature = new Signature((Hash)signature.R, (Hash)signature.S);
            api.CommitTransaction(tr);
        }

        private static async Task ReadyAsync()
        {
            await Task.Run(()=>Console.WriteLine("Initialized"));
        }

        private static async Task LogAsync(LogMessage arg)
        {
            await Task.Run(() => Console.WriteLine(arg.Message));
        }
    }
}
