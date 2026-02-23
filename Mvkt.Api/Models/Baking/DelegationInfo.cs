namespace Mvkt.Api.Models
{
    /// <summary>
    /// Complete delegation and staking information for an account
    /// </summary>
    public class DelegationInfo
    {
        /// <summary>
        /// Summary of delegation and staking status
        /// </summary>
        public DelegationSummary Summary { get; set; }

        /// <summary>
        /// Actual rewards received vs expected
        /// </summary>
        public ActualRewards ActualRewards { get; set; }

        /// <summary>
        /// Estimated date of next payout (if predictable)
        /// </summary>
        public DateTime? EstimatedNextPayout { get; set; }

        /// <summary>
        /// Average payout interval in days
        /// </summary>
        public double? AvgPayoutIntervalDays { get; set; }

        /// <summary>
        /// Last vote information (optional)
        /// </summary>
        public LastVoteInfo LastVote { get; set; }
    }
}
