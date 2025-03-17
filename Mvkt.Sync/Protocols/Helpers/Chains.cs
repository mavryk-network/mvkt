﻿namespace Mvkt.Sync.Protocols
{
    static class Chains
    {
        public static string GetName(string chainId) => chainId switch
        {
            "NetXdQprcVkpaWU" => "mainnet",
            "NetXnHfVqm9iesp" => "basenet",
            "NetXUrNc8uioxP8" => "atlasnet",
            "NetXRp4kyGKJTuB" => "weeklynet",
            _ => "private"
        };
    }
}
