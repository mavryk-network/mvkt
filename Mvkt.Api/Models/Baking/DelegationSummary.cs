namespace Mvkt.Api.Models
{
    /// <summary>
    /// Summary of account's delegation and staking status
    /// </summary>
    public class DelegationSummary
    {
        /// <summary>
        /// Whether the account is currently delegating
        /// </summary>
        public bool IsDelegating { get; set; }

        /// <summary>
        /// Current delegated validator
        /// </summary>
        public Alias DelegatedValidator { get; set; }

        /// <summary>
        /// Amount delegated
        /// </summary>
        public long DelegatedBalance { get; set; }

        /// <summary>
        /// Delegation start time (ISO 8601)
        /// </summary>
        public DateTime? DelegationTime { get; set; }

        /// <summary>
        /// Whether the account is currently staking
        /// </summary>
        public bool IsStaking { get; set; }

        /// <summary>
        /// List of validators the account is staked with
        /// </summary>
        public List<StakedValidatorInfo> StakedValidators { get; set; }

        /// <summary>
        /// Total amount staked across all validators
        /// </summary>
        public long TotalStakedBalance { get; set; }

        /// <summary>
        /// Total rewards earned (delegation + staking)
        /// </summary>
        public long TotalRewardsEarned { get; set; }

        /// <summary>
        /// Rewards breakdown by validator
        /// </summary>
        public List<ValidatorRewardSummary> RewardsByValidator { get; set; }

        /// <summary>
        /// Rewards breakdown by cycle
        /// </summary>
        public List<CycleRewardSummary> RewardsByCycle { get; set; }

        /// <summary>
        /// Percentage of balance that is staked
        /// </summary>
        public double StakingPercentage { get; set; }

        /// <summary>
        /// Percentage of balance that is delegated
        /// </summary>
        public double DelegationPercentage { get; set; }

        /// <summary>
        /// Total number of validators used (past + current)
        /// </summary>
        public int TotalValidatorsUsed { get; set; }

        /// <summary>
        /// Number of currently active validators
        /// </summary>
        public int CurrentValidatorCount { get; set; }

        /// <summary>
        /// Number of past validators no longer used
        /// </summary>
        public int PastValidatorCount { get; set; }

        /// <summary>
        /// First cycle with rewards
        /// </summary>
        public int? FirstRewardCycle { get; set; }

        /// <summary>
        /// Last cycle with rewards
        /// </summary>
        public int? LastRewardCycle { get; set; }

        /// <summary>
        /// Total cycles that earned rewards
        /// </summary>
        public int TotalCyclesWithRewards { get; set; }
    }
}
