﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Mvkt.Data.Models;

namespace Mvkt.Sync.Protocols.Proto14
{
    partial class ProtoActivator : Proto13.ProtoActivator
    {
        public ProtoActivator(ProtocolHandler proto) : base(proto) { }

        protected override void SetParameters(Protocol protocol, JToken parameters)
        {
            base.SetParameters(protocol, parameters);
            protocol.Dictator = parameters["testnet_dictator"]?.Value<string>();
        }

        protected override void UpgradeParameters(Protocol protocol, Protocol prev)
        {
            if (Cache.AppState.GetChainId() == "NetXnHfVqm9iesp") // basenet
            {
                protocol.BlocksPerVoting = prev.BlocksPerCycle;
                protocol.Dictator = "mv1Rck3MEfcu8zZ8GDtnu9LjknmYv8bBPJea"; // oxhead_testnet_baker
            }
        }

        protected override async Task MigrateContext(AppState state)
        {
            // var block = await Cache.Blocks.CurrentAsync();
            // var account = await Cache.Accounts.GetAsync("tz1X81bCXPtMiHu1d4UZF4GPhMPkvkp56ssb");

            // Db.TryAttach(account);
            // account.FirstLevel = Math.Min(account.FirstLevel, state.Level);
            // account.LastLevel = state.Level;
            // account.Balance += 3_000_000_000L;
            // account.MigrationsCount++;

            // block.Operations |= Operations.Migrations;
            // Db.MigrationOps.Add(new MigrationOperation
            // {
            //     Id = Cache.AppState.NextOperationId(),
            //     Block = block,
            //     Level = block.Level,
            //     Timestamp = block.Timestamp,
            //     Account = account,
            //     Kind = MigrationKind.ProposalInvoice,
            //     BalanceChange = 3_000_000_000L
            // });

            // state.MigrationOpsCount++;

            // Cache.Statistics.Current.TotalCreated += 3_000_000_000L;

            if (state.ChainId == "NetXnHfVqm9iesp") // basenet
            {
                var votingPeriod = await Cache.Periods.GetAsync(58);
                Db.TryAttach(votingPeriod);
                votingPeriod.LastLevel = 0;
                state.VotingPeriod = 58;
                state.VotingEpoch = 58;
            }
        }

        protected override async Task RevertContext(AppState state)
        {
            var block = await Cache.Blocks.CurrentAsync();

            var invoice = await Db.MigrationOps
                .AsNoTracking()
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Level == block.Level && x.Kind == MigrationKind.ProposalInvoice);

            Db.TryAttach(invoice.Account);
            Cache.Accounts.Add(invoice.Account);

            invoice.Account.Balance -= invoice.BalanceChange;
            invoice.Account.MigrationsCount--;

            Db.MigrationOps.Remove(invoice);
            Cache.AppState.ReleaseOperationId();

            state.MigrationOpsCount--;
        }
    }
}
