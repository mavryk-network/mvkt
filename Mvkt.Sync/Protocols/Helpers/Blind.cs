﻿using Blake2Fast;
using Netmavryk.Encoding;

namespace Mvkt.Sync.Protocols
{
    static class Blind
    {
        static readonly byte[] Prefix = new byte[] { 1, 1, 75, 4 };

        public static string GetBlindedAddress(string address, string secret)
        {
            var pkh = Base58.Parse(address).GetBytes(3, 20);
            var key = Hex.Parse(secret);
            var blind = Blake2b.ComputeHash(20, key, pkh);

            return Base58.Convert(blind, Prefix);
        }
    }
}
