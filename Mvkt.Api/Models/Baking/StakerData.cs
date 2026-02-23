namespace Mvkt.Api.Models
{
    /// <summary>
    /// Staker's balance at a specific baker (validator). Used in delegation info.
    /// </summary>
    public class StakerData
    {
        /// <summary>
        /// Baker (validator) address
        /// </summary>
        public string BakerAddress { get; set; }

        /// <summary>
        /// Baker alias (off-chain name)
        /// </summary>
        public string BakerAlias { get; set; }

        /// <summary>
        /// Staked balance at this baker
        /// </summary>
        public long StakedBalance { get; set; }
    }
}
