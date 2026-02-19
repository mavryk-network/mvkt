namespace Mvkt.Api.Models
{
    /// <summary>
    /// Actual rewards received compared to expected rewards
    /// </summary>
    public class ActualRewards
    {
        /// <summary>
        /// Total actual rewards received (micro tez)
        /// </summary>
        public long TotalActualRewards { get; set; }

        /// <summary>
        /// Delegation payouts received (micro tez)
        /// </summary>
        public long DelegationPayouts { get; set; }

        /// <summary>
        /// Number of delegation payout transactions
        /// </summary>
        public int DelegationPayoutCount { get; set; }

        /// <summary>
        /// Staking rewards auto-restaked (micro tez)
        /// </summary>
        public long StakingRewardsRestaked { get; set; }

        /// <summary>
        /// Number of staking restake events
        /// </summary>
        public int StakingRestakeCount { get; set; }

        /// <summary>
        /// Expected delegation rewards (micro tez)
        /// </summary>
        public long ExpectedDelegationRewards { get; set; }

        /// <summary>
        /// Expected staking rewards (micro tez)
        /// </summary>
        public long ExpectedStakingRewards { get; set; }

        /// <summary>
        /// Expected total rewards (micro tez)
        /// </summary>
        public long ExpectedTotalRewards { get; set; }

        /// <summary>
        /// Pending delegation rewards not yet paid (micro tez)
        /// </summary>
        public long PendingDelegation { get; set; }

        /// <summary>
        /// Pending staking rewards (micro tez)
        /// </summary>
        public long PendingStaking { get; set; }

        /// <summary>
        /// Total pending rewards (micro tez)
        /// </summary>
        public long PendingTotal { get; set; }

        /// <summary>
        /// Reward tracking per validator
        /// </summary>
        public List<ValidatorRewardTracking> ByValidator { get; set; }

        /// <summary>
        /// All staking reward events
        /// </summary>
        public List<StakingRewardEvent> AllStakingRewardEvents { get; set; }

        /// <summary>
        /// First reward date (ISO 8601)
        /// </summary>
        public DateTime? FirstRewardDate { get; set; }

        /// <summary>
        /// Last reward date (ISO 8601)
        /// </summary>
        public DateTime? LastRewardDate { get; set; }
    }
}
