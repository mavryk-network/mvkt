namespace Mvkt.Api.Models
{
    /// <summary>
    /// Detailed reward tracking for a specific validator
    /// </summary>
    public class ValidatorRewardTracking
    {
        /// <summary>
        /// Validator address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Validator alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Expected staking rewards
        /// </summary>
        public long ExpectedStakingRewards { get; set; }

        /// <summary>
        /// Expected delegation rewards
        /// </summary>
        public long ExpectedDelegationRewards { get; set; }

        /// <summary>
        /// Expected total rewards
        /// </summary>
        public long ExpectedTotalGross { get; set; }

        /// <summary>
        /// Actual delegation payouts received
        /// </summary>
        public long ActualDelegationPayouts { get; set; }

        /// <summary>
        /// Actual staking rewards restaked
        /// </summary>
        public long ActualStakingRestaked { get; set; }

        /// <summary>
        /// Actual total received
        /// </summary>
        public long ActualTotalReceived { get; set; }

        /// <summary>
        /// Pending delegation rewards
        /// </summary>
        public long PendingDelegation { get; set; }

        /// <summary>
        /// Pending staking rewards
        /// </summary>
        public long PendingStaking { get; set; }

        /// <summary>
        /// Total pending rewards
        /// </summary>
        public long PendingTotal { get; set; }

        /// <summary>
        /// Payment status: fully_paid, underpaid, overpaid, pending
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Payment percentage (actual / expected * 100)
        /// </summary>
        public double PaymentPercentage { get; set; }

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
        /// Staking rewards by cycle (validator is the one being tracked by this record)
        /// </summary>
        public List<StakingRewardEvent> StakingRewardsByCycle { get; set; }

        /// <summary>
        /// Number of staking restake events
        /// </summary>
        public int StakingRestakeEventCount { get; set; }

        /// <summary>
        /// Delegation payment status by cycle
        /// </summary>
        public List<CyclePaymentStatus> DelegationByCycle { get; set; }
    }
}
