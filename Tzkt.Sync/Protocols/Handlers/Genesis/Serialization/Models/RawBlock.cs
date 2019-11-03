﻿using System;
using System.Text.Json.Serialization;

namespace Tzkt.Sync.Protocols.Genesis
{
    class RawBlock : IBlock
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; }

        [JsonPropertyName("chain_id")]
        public string Chain { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("header")]
        public RawBlockHeader Header { get; set; }

        [JsonPropertyName("metadata")]
        public RawBlockMetadata Metadata { get; set; }

        #region IBlock
        public int Level => Header.Level;
        public string Predecessor => Header.Predecessor;
        public int OperationsCount => 0;
        #endregion

        #region validation
        public bool IsValidFormat() =>
            !string.IsNullOrEmpty(Protocol) &&
            !string.IsNullOrEmpty(Chain) &&
            !string.IsNullOrEmpty(Hash) &&
            Header?.IsValidFormat() == true &&
            Metadata?.IsValidFormat() == true;
        #endregion
    }

    class RawBlockHeader
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("predecessor")]
        public string Predecessor { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        #region validation
        public bool IsValidFormat() =>
            Level >= 0 &&
            !string.IsNullOrEmpty(Predecessor) &&
            Timestamp != DateTime.MinValue;
        #endregion
    }

    class RawBlockMetadata
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; }

        [JsonPropertyName("next_protocol")]
        public string NextProtocol { get; set; }

        #region validation
        public bool IsValidFormat() =>
            !string.IsNullOrEmpty(Protocol) &&
            !string.IsNullOrEmpty(NextProtocol);
        #endregion
    }
}