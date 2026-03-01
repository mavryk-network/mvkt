using System.Net.Http;
using System.Threading.Tasks;
using Dynamic.Json;
using Dynamic.Json.Extensions;
using Xunit;

namespace Mvkt.Api.Tests.Api
{
    public class TestHomeQueries : IClassFixture<SettingsFixture>
    {
        readonly HttpClient Client;

        public TestHomeQueries(SettingsFixture settings)
        {
            Client = settings.Client;
        }

        [Fact]
        public async Task TestHomeStats()
        {
            var res = await Client.GetJsonAsync("/v1/home");
            Assert.True(res is DJsonObject);
            if (res.networkRewardsData == null) return;

            var nr = res.networkRewardsData;
            Assert.Null(nr.cycleRewardSummaries);

            long totalRewardsAllTime = (long)nr.totalRewardsAllTime;
            long totalBlockRewards = (long)nr.totalBlockRewards;
            long totalEndorsementRewards = (long)nr.totalEndorsementRewards;
            long totalBlockFees = (long)nr.totalBlockFees;
            Assert.Equal(totalRewardsAllTime, totalBlockRewards + totalEndorsementRewards + totalBlockFees);

            Assert.True((double)nr.averageRewardsPerCycle >= 0);
        }

        [Fact]
        public async Task TestHomeNetworkRewardsDataWithCycleRewardSummaries()
        {
            var res = await Client.GetJsonAsync("/v1/home?cycleRewardSummaries=true");
            if (res is not DJsonObject || res.networkRewardsData == null)
                return;

            var nr = res.networkRewardsData;
            var cycleRewardSummaries = nr.cycleRewardSummaries as DJsonArray;
            Assert.NotNull(cycleRewardSummaries);
            if (cycleRewardSummaries.Count > 0)
                Assert.Equal((double)nr.totalRewardsAllTime / cycleRewardSummaries.Count, (double)nr.averageRewardsPerCycle, 0);

            if (cycleRewardSummaries.Count > 0)
            {
                dynamic first = (cycleRewardSummaries as dynamic)[0];
                Assert.NotNull(first);
                Assert.True(first.cycle is not null);
                Assert.True(first.totalBlockRewards is not null);
                Assert.True(first.totalEndorsementRewards is not null);
                Assert.True(first.totalBlockFees is not null);
                Assert.True(first.totalRewards is not null);
                Assert.True(first.activeBakers is not null);
            }
        }

        [Fact]
        public async Task TestHomeStatsWithQuote()
        {
            var res = await Client.GetJsonAsync("/v1/home?quote=usd");
            Assert.True(res is DJsonObject);
            Assert.NotNull(res.priceChart);
            var priceChart = res.priceChart as DJsonArray;
            Assert.NotNull(priceChart);
            Assert.True(priceChart.Count > 0);
        }

        [Fact]
        public async Task TestHomeBlocks()
        {
            var res = await Client.GetJsonAsync("/v1/home/blocks");

            Assert.True(res is DJsonArray);
        }

        [Fact]
        public async Task TestHomeAccounts()
        {
            var res = await Client.GetJsonAsync("/v1/home/accounts");

            Assert.True(res is DJsonArray);
        }

        [Fact]
        public async Task TestHomeBakers()
        {
            var res = await Client.GetJsonAsync("/v1/home/bakers");

            Assert.True(res is DJsonArray);
        }

        [Fact]
        public async Task TestHomeAssets()
        {
            var res = await Client.GetJsonAsync("/v1/home/assets");

            Assert.True(res is DJsonArray);
        }
    }
}

