﻿using System.Text.Json;
using Mvkt.Sync.Services;

namespace Mvkt.Sync.Protocols.Proto12
{
    class Rpc : Proto6.Rpc
    {
        public Rpc(MavrykNode node) : base(node) { }

        public override Task<JsonElement> GetStakeDistribution(int block, int cycle)
            => Node.GetAsync($"chains/main/blocks/{block}/context/raw/json/cycle/{cycle}/selected_stake_distribution");

        public override Task<JsonElement> GetDelegateParticipationAsync(int level, string address)
            => Node.GetAsync($"chains/main/blocks/{level}/context/delegates/{address}/participation");

        public override Task<JsonElement> GetLevelBakingRightsAsync(int block, int level, int maxRound)
            => Node.GetAsync($"chains/main/blocks/{block}/helpers/baking_rights?level={level}&max_round={maxRound}&all=true");
    }
}
