namespace Mvkt.Api.Models
{
    /// <summary>
    /// A single staking reward event
    /// </summary>
    public class StakingRewardEvent
    {
        /// <summary>
        /// Cycle when the reward was received
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// Reward amount (micro tez)
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Timestamp of the reward (ISO 8601)
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Validator address
        /// </summary>
        public string ValidatorAddress { get; set; }

        /// <summary>
        /// Validator alias
        /// </summary>
        public string ValidatorAlias { get; set; }
    }
}
