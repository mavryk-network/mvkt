namespace Mvkt.Api.Models
{
    /// <summary>
    /// Baker statistics based on historical rewards
    /// </summary>
    public class BakerStats
    {
        /// <summary>
        /// Baker alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Current APY metrics for the baker
        /// </summary>
        public BakerApy Apy { get; set; }

        /// <summary>
        /// Luck percentage (actual rewards / expected rewards * 100)
        /// Values &gt; 100% indicate good luck, &lt; 100% indicate bad luck
        /// </summary>
        public double Luck { get; set; }

        /// <summary>
        /// Performance percentage (successful operations / total opportunities * 100)
        /// </summary>
        public double Performance { get; set; }

        /// <summary>
        /// Reliability percentage (blocks + attestations / expected blocks + expected attestations * 100)
        /// </summary>
        public double Reliability { get; set; }

        /// <summary>
        /// Total expected rewards (micro tez)
        /// </summary>
        public long TotalExpectedRewards { get; set; }

        /// <summary>
        /// Total actual rewards received (micro tez)
        /// </summary>
        public long TotalActualRewards { get; set; }

        /// <summary>
        /// Cycle if request was for a single cycle; null for lifetime.
        /// </summary>
        public int? Cycle { get; set; }

        /// <summary>
        /// Number of cycles included in aggregation (1 for single-cycle, else up to cycles param).
        /// </summary>
        public int CyclesUsed { get; set; }

        /// <summary>
        /// Reward-derived KPIs and efficiency + rewards summary. Null if baker has no cycles.
        /// </summary>
        public BakerStatsKpis Kpis { get; set; }
    }

    /// <summary>
    /// Aggregated KPIs and rewards summary for validator Cycle/Lifetime tabs.
    /// </summary>
    public class BakerStatsKpis
    {
        #region Reward-derived KPIs
        public long TotalIncome { get; set; }
        public long ExtraRewards { get; set; }
        public long LostRewards { get; set; }
        public long SlashedRewards { get; set; }
        public long TotalSlashed { get; set; }
        public long BlocksBaked { get; set; }
        public long BlocksProposed { get; set; }
        public long TotalBlockRewards { get; set; }
        public long TotalEndorsementRewards { get; set; }
        public long EndorsementsMade { get; set; }
        public long EndorsementsMissed { get; set; }
        /// <summary>Expected to delegators + co-stakers (client: pending = ExpectedDistribution - payouts).</summary>
        public long ExpectedDistribution { get; set; }
        #endregion

        #region Efficiency
        public double TechnicalReliability { get; set; }
        public double MonetaryPerformance { get; set; }
        public double FairEfficiency { get; set; }
        public double LuckRatio { get; set; }
        public long TotalExpectedRewards { get; set; }
        public long TotalActualRewards { get; set; }
        public int MissedRights { get; set; }
        #endregion

        #region Rewards summary
        public long TotalRewards { get; set; }
        public long TotalBlockFees { get; set; }
        public long TotalRevelationRewards { get; set; }
        public long MissedBlocks { get; set; }
        public long MissedEndorsements { get; set; }
        public double PerformanceRate { get; set; }
        public double AvgRewardsPerCycle { get; set; }
        public long TotalRewardsDelegated { get; set; }
        public long TotalRewardsStakedShared { get; set; }
        public long TotalRewardsOwnStake { get; set; }
        public long TotalEdgeFees { get; set; }
        public double DelegatorSharePercent { get; set; }
        public double CoStakerSharePercent { get; set; }
        public double ValidatorSharePercent { get; set; }
        #endregion
    }
}

