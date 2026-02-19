namespace Mvkt.Api.Services
{
    /// <summary>
    /// Optional delegation configuration (e.g. validator payout addresses).
    /// Bound from configuration section "Delegation".
    /// In appsettings.json add: "Delegation": { "ValidatorPayoutAddresses": { "validator_address": [ "payout_address" ] } }
    /// </summary>
    public class DelegationConfig
    {
        /// <summary>
        /// Maps validator address (baker) to a list of alternative addresses used to send delegation payouts.
        /// Transactions from these addresses to the user are attributed to the validator.
        /// </summary>
        public Dictionary<string, List<string>> ValidatorPayoutAddresses { get; set; } = new();
    }
}
