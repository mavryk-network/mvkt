namespace Mvkt.Api.Models
{
    /// <summary>
    /// Information about a validator the account is staked with
    /// </summary>
    public class StakedValidatorInfo
    {
        /// <summary>
        /// Validator information
        /// </summary>
        public Alias Baker { get; set; }

        /// <summary>
        /// Amount staked with this validator
        /// </summary>
        public long StakedBalance { get; set; }
    }
}
