namespace Mvkt.Api.Models
{
    /// <summary>
    /// Staking reward for a specific cycle
    /// </summary>
    public class CycleStakingReward
    {
        /// <summary>
        /// Cycle number
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
    }
}
