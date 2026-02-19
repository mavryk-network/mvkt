namespace Mvkt.Api.Models
{
    /// <summary>
    /// Information about the last vote cast by the account
    /// </summary>
    public class LastVoteInfo
    {
        /// <summary>
        /// Proposal hash
        /// </summary>
        public string Proposal { get; set; }

        /// <summary>
        /// Vote: yay, nay, pass
        /// </summary>
        public string Vote { get; set; }

        /// <summary>
        /// Voting period
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Timestamp of the vote (ISO 8601)
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
