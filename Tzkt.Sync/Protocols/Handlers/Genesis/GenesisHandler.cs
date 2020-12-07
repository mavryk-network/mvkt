﻿using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Tzkt.Data;
using Tzkt.Data.Models;
using Tzkt.Sync.Services;
using Tzkt.Sync.Protocols.Genesis;

namespace Tzkt.Sync.Protocols
{
    class GenesisHandler : ProtocolHandler
    {
        public override string Protocol => "Genesis";
        public override IDiagnostics Diagnostics { get; }
        public override IValidator Validator { get; }
        public override IRpc Rpc { get; }

        public GenesisHandler(TezosNode node, TzktContext db, CacheService cache, QuotesService quotes, IConfiguration config, ILogger<GenesisHandler> logger)
            : base(node, db, cache, quotes, config, logger)
        {
            Diagnostics = new Diagnostics();
            Validator = new Validator(this);
            Rpc = new Rpc(node);
        }

        public override Task Precache(JsonElement block) => Task.CompletedTask;

        public override Task InitProtocol(JsonElement block)
        {
            var protocol = new Protocol
            {
                Hash = block.RequiredString("protocol"),
                Code = -1,
                FirstLevel = block.Required("header").RequiredInt32("level"),
                LastLevel = block.Required("header").RequiredInt32("level")
            };

            Db.Protocols.Add(protocol);
            Cache.Protocols.Add(protocol);
            return Task.CompletedTask;
        }

        public override Task InitProtocol()
        {
            return Task.CompletedTask;
        }

        public override async Task Commit(JsonElement block)
        {
            var blockCommit = await BlockCommit.Apply(this, block);

            await StatisticsCommit.Apply(this);

            await StateCommit.Apply(this, blockCommit.Block, block);
        }

        public override async Task Revert()
        {
            var currBlock = await Cache.Blocks.CurrentAsync();

            await StatisticsCommit.Revert(this);

            await BlockCommit.Revert(this, currBlock);

            await StateCommit.Revert(this, currBlock);
        }
    }
}
