namespace Mvkt.Api.Models
{
    /// <summary>
    /// Summary of rewards for a specific cycle
    /// </summary>
    public class CycleRewardSummary
    {
        /// <summary>
        /// Cycle number
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// Total rewards for this cycle
        /// </summary>
        public long Rewards { get; set; }

        /// <summary>
        /// Delegated balance during this cycle
        /// </summary>
        public long DelegatedBalance { get; set; }

        /// <summary>
        /// Staked balance during this cycle
        /// </summary>
        public long StakedBalance { get; set; }

        /// <summary>
        /// Validator for this cycle
        /// </summary>
        public Alias Validator { get; set; }

        /// <summary>
        /// Staking rewards for this cycle
        /// </summary>
        public long StakingRewards { get; set; }

        /// <summary>
        /// Delegation rewards for this cycle
        /// </summary>
        public long DelegationRewards { get; set; }
    }
}
