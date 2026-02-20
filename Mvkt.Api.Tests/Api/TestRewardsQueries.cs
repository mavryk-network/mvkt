using System.Net.Http;
using System.Threading.Tasks;
using Dynamic.Json;
using Dynamic.Json.Extensions;
using Xunit;

namespace Mvkt.Api.Tests.Api
{
    public class TestRewardsQueries : IClassFixture<SettingsFixture>
    {
        readonly HttpClient Client;
        readonly SettingsFixture Settings;
        
        public TestRewardsQueries(SettingsFixture settings)
        {
            Client = settings.Client;
            Settings = settings;
        }

        [Fact]
        public async Task TestBakerRewardsCount()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}/count");

            Assert.True(res is DJsonValue);
        }

        [Fact]
        public async Task TestBakerRewards()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}");

            Assert.True(res is DJsonArray);
        }

        [Fact]
        public async Task TestBakerStats()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}/stats");

            Assert.True(res is DJsonObject);

            Assert.True((double)res.luck >= 0);
            Assert.True((double)res.performance >= 0 && (double)res.performance <= 100);
            Assert.True((double)res.reliability >= 0 && (double)res.reliability <= 100);
            Assert.True((long)res.totalExpectedRewards >= 0);
            Assert.True((long)res.totalActualRewards >= 0);

            Assert.True(res.apy is DJsonObject);
            Assert.True(((double)res.apy.ownStakeApy >= 0 && (double)res.apy.externalStakeApy >= 0 && (double)res.apy.delegationApy >= 0));
            Assert.True(((double)res.apy.ownStakeApy < 1000 && (double)res.apy.externalStakeApy < 1000 && (double)res.apy.delegationApy < 1000));

            Assert.True((int)res.cyclesUsed > 0);
            Assert.True(res.kpis is DJsonObject);
        }

        [Fact]
        public async Task TestBakerStatsWithCycle()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}/stats?cycle={Settings.Cycle}");

            Assert.True(res is DJsonObject);
            var cyclesUsed = res.cyclesUsed != null ? (int)res.cyclesUsed : 0;
            Assert.True(cyclesUsed == 1);
            Assert.True((int)res.cycle == Settings.Cycle);
            Assert.True(res.kpis is DJsonObject);
        }

        [Fact]
        public async Task TestBakerStatsWithCyclesLimit()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}/stats?cyclesLimit=5");

            Assert.True(res is DJsonObject);
            Assert.True((int)res.cyclesUsed == 5);
            Assert.Null(res.cycle);
        }

        [Fact]
        public async Task TestBakerStatsInvalidBakerAddress()
        {
            var response = await Client.GetAsync("/v1/rewards/bakers/mv111111111111111111111111111111111111/stats");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TestBakerRewardsByCycle()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/bakers/{Settings.Baker}/{Settings.Cycle}");

            Assert.True(res is DJsonObject);
        }

        [Fact]
        public async Task TestDelegatorRewardsCount()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/count");

            Assert.True(res is DJsonValue);
        }

        [Fact]
        public async Task TestDelegatorRewards()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Delegator}");

            Assert.True(res is DJsonArray);
        }

        [Fact]
        public async Task TestDelegatorRewardsByCycle()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Delegator}/{Settings.Cycle}");

            Assert.True(res is DJsonObject);
        }

        [Fact]
        public async Task TestRewardSplit()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/split/{Settings.Baker}/{Settings.Cycle}");

            Assert.True(res is DJsonObject);
        }

        [Fact]
        public async Task TestRewardSplitDelegator()
        {
            var res = await Client.GetJsonAsync($"/v1/rewards/split/{Settings.Baker}/{Settings.Cycle}/{Settings.Delegator}");

            Assert.True(res is DJsonObject);
        }
    }
}
