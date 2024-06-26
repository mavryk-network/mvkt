﻿using Mvkt.Data.Models;

namespace Mvkt.Sync.Protocols.Proto13
{
    class VotingCommit : Proto8.VotingCommit
    {
        public VotingCommit(ProtocolHandler protocol) : base(protocol) { }

        protected override long GetVotingPower(Data.Models.Delegate baker, Block block, Protocol protocol)
        {
            return baker.StakingBalance;
        }
    }
}
