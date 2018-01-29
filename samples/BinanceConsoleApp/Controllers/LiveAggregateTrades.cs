﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Binance;
using Binance.Cache;
using Binance.Cache.Events;
using Microsoft.Extensions.DependencyInjection;

namespace BinanceConsoleApp.Controllers
{
    internal class LiveAggregateTrades : IHandleCommand
    {
        public Task<bool> HandleAsync(string command, CancellationToken token = default)
        {
            if (!command.StartsWith("live ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            var args = command.Split(' ');

            var endpoint = string.Empty;
            if (args.Length > 1)
            {
                endpoint = args[1];
            }

            string symbol = Symbol.BTC_USDT;
            if (args.Length > 2)
            {
                symbol = args[2];
            }

            if (!endpoint.Equals("aggTrades", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            if (Program.LiveTask != null)
            {
                lock (Program.ConsoleSync)
                {
                    Console.WriteLine("! A live task is currently active ...use 'live off' to disable.");
                }
                return Task.FromResult(true);
            }

            Program.LiveTokenSource = new CancellationTokenSource();

            Program.AggregateTradeCache = Program.ServiceProvider.GetService<IAggregateTradeCache>();

            Program.LiveTask = Task.Run(() =>
            {
                Program.AggregateTradeCache.SubscribeAndStreamAsync(symbol, e => { Program.Display(e.LatestTrade()); }, Program.LiveTokenSource.Token);
            }, token);

            lock (Program.ConsoleSync)
            {
                Console.WriteLine();
                Console.WriteLine($"  ...live aggregate trades feed enabled for symbol: {symbol} ...use 'live off' to disable.");
            }

            return Task.FromResult(true);
        }
    }
}
