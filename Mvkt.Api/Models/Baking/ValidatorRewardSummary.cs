namespace Mvkt.Api.Models
{
    /// <summary>
    /// Summary of rewards from a specific validator
    /// </summary>
    public class ValidatorRewardSummary
    {
        /// <summary>
        /// Validator information
        /// </summary>
        public Alias Validator { get; set; }

        /// <summary>
        /// Total rewards from this validator
        /// </summary>
        public long TotalRewards { get; set; }

        /// <summary>
        /// Number of cycles with rewards
        /// </summary>
        public int CycleCount { get; set; }

        /// <summary>
        /// First cycle with rewards
        /// </summary>
        public int FirstCycle { get; set; }

        /// <summary>
        /// Last cycle with rewards
        /// </summary>
        public int LastCycle { get; set; }

        /// <summary>
        /// Total staking rewards (auto-restaked)
        /// </summary>
        public long TotalStakingRewards { get; set; }

        /// <summary>
        /// Total delegation rewards (paid out)
        /// </summary>
        public long TotalDelegationRewards { get; set; }

        /// <summary>
        /// Whether this is a current validator
        /// </summary>
        public bool IsCurrentValidator { get; set; }
    }
}
