namespace Mvkt.Api.Models
{
    /// <summary>
    /// Payment status for a specific cycle
    /// </summary>
    public class CyclePaymentStatus
    {
        /// <summary>
        /// Cycle number
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// Expected reward for this cycle (micro tez)
        /// </summary>
        public long ExpectedReward { get; set; }

        /// <summary>
        /// Payment status: paid, underpaid, overpaid, pending
        /// </summary>
        public string Status { get; set; }

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
