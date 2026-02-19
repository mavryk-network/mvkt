using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dynamic.Json;
using Dynamic.Json.Extensions;
using Xunit;

namespace Mvkt.Api.Tests.Api
{
    public class TestDelegationSummaryQueries : IClassFixture<SettingsFixture>
    {
        readonly SettingsFixture Settings;
        readonly HttpClient Client;

        public TestDelegationSummaryQueries(SettingsFixture settings)
        {
            Settings = settings;
            Client = settings.Client;
        }

        [Fact]
        public async Task TestSummary_ReturnsFullStructure()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary");

            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.NotNull(res.actualRewards);

            Assert.NotNull(res.summary.isDelegating);
            Assert.NotNull(res.summary.isStaking);
            Assert.NotNull(res.summary.totalRewardsEarned);
            Assert.NotNull(res.summary.rewardsByValidator);
            Assert.NotNull(res.summary.rewardsByCycle);
            Assert.NotNull(res.summary.totalValidatorsUsed);
            Assert.NotNull(res.summary.currentValidatorCount);
            Assert.NotNull(res.summary.pastValidatorCount);
            Assert.NotNull(res.summary.totalCyclesWithRewards);

            Assert.NotNull(res.actualRewards.totalActualRewards);
            Assert.NotNull(res.actualRewards.delegationPayouts);
            Assert.NotNull(res.actualRewards.stakingRewardsRestaked);
            Assert.NotNull(res.actualRewards.expectedTotalRewards);
            Assert.NotNull(res.actualRewards.pendingDelegation);
            Assert.NotNull(res.actualRewards.byValidator);

            if (res.summary.rewardsByCycle is DJsonArray arr && arr.Count > 0)
            {
                Assert.NotNull(res.summary.firstRewardCycle);
                Assert.NotNull(res.summary.lastRewardCycle);
                Assert.True((int)res.summary.totalCyclesWithRewards >= 0);
            }
        }

        [Fact]
        public async Task TestSummary_NonExistentAddress_Returns200WithEmptyInfo()
        {
            const string nonExistent = "/v1/rewards/delegators/mv1V4h45W3p4e1sjSBvRkK2uYbvkTnSuHg1c/summary";
            var response = await Client.GetAsync(nonExistent);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            dynamic res = await Client.GetJsonAsync(nonExistent);
            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.NotNull(res.actualRewards);
            Assert.False((bool)res.summary.isDelegating);
            Assert.False((bool)res.summary.isStaking);
            Assert.NotNull(res.summary.stakedValidators);
            Assert.True(res.summary.stakedValidators is DJsonArray);
            Assert.Equal(0, ((DJsonArray)res.summary.stakedValidators).Count);
            Assert.Equal(0L, (long)res.summary.totalStakedBalance);
            Assert.Equal(0L, (long)res.summary.totalRewardsEarned);
            Assert.NotNull(res.actualRewards.byValidator);
            Assert.True(res.actualRewards.byValidator is DJsonArray);
            Assert.Equal(0, ((DJsonArray)res.actualRewards.byValidator).Count);
        }

        [Fact]
        public async Task TestDelegationSummary_WithLegacyParameter()
        {
            var resLegacyTrue = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary?legacy=true");
            var resLegacyFalse = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary?legacy=false");

            Assert.True(resLegacyTrue is DJsonObject);
            Assert.True(resLegacyFalse is DJsonObject);
            Assert.NotNull(resLegacyTrue.summary);
            Assert.NotNull(resLegacyFalse.summary);
            Assert.NotNull(resLegacyTrue.actualRewards);
            Assert.NotNull(resLegacyFalse.actualRewards);
        }

        [Fact]
        public async Task TestContractDelegationSummary()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Originator}/summary");

            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.NotNull(res.actualRewards);
            Assert.NotNull(res.summary.isDelegating);
            Assert.NotNull(res.summary.isStaking);
        }

        [Fact]
        public async Task TestSummary_RewardsByCycle_OrderAndStructure()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary");
            var rewardsByCycle = res.summary.rewardsByCycle;
            if (rewardsByCycle is DJsonArray arr && arr.Count > 0)
            {
                dynamic first = arr.First();
                Assert.NotNull(first.cycle);
                Assert.NotNull(first.rewards);
                Assert.NotNull(first.delegatedBalance);
                Assert.NotNull(first.stakedBalance);
                Assert.NotNull(first.validator);

                if (arr.Count >= 2)
                {
                    for (int i = 0; i < arr.Count - 1; i++)
                    {
                        int current = (int)((dynamic)arr.ElementAt(i)).cycle;
                        int next = (int)((dynamic)arr.ElementAt(i + 1)).cycle;
                        Assert.True(current >= next, $"rewardsByCycle should be descending: at index {i} cycle={current}, at {i + 1} cycle={next}");
                    }
                }
            }
        }

        [Fact]
        public async Task TestSummary_ActualRewards_AllStakingRewardEvents_Structure()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary");
            Assert.NotNull(res.actualRewards);
            var events = res.actualRewards.allStakingRewardEvents;
            Assert.NotNull(events);
            Assert.True(events is DJsonArray);
            if (((DJsonArray)events).Count > 0)
            {
                dynamic ev = ((DJsonArray)events).First();
                Assert.NotNull(ev.cycle);
                Assert.NotNull(ev.amount);
                Assert.NotNull(ev.validatorAddress);
            }
        }

        [Fact]
        public async Task TestSummary_ByValidator()
        {
            var validPaymentStatuses = new[] { "fully_paid", "pending", "underpaid", "overpaid" };
            var validCycleStatuses = new[] { "paid", "pending", "underpaid", "overpaid" };

            dynamic res = await Client.GetJsonAsync($"/v1/rewards/delegators/{Settings.Baker}/summary");
            Assert.NotNull(res.actualRewards);
            var byValidator = res.actualRewards.byValidator;

            if (byValidator is DJsonArray arr && arr.Count > 0)
            {
                dynamic first = arr.First();
                Assert.NotNull(first.address);
                Assert.NotNull(first.paymentStatus);
                Assert.NotNull(first.expectedDelegationRewards);
                Assert.NotNull(first.actualDelegationPayouts);
                Assert.NotNull(first.pendingDelegation);

                foreach (dynamic v in arr)
                {
                    string status = (string)v.paymentStatus;
                    Assert.NotNull(status);
                    Assert.Contains(status, validPaymentStatuses);

                    var delegationByCycle = v.delegationByCycle;
                    Assert.NotNull(delegationByCycle);
                    if (delegationByCycle is DJsonArray dcArr && dcArr.Count > 0)
                    {
                        foreach (dynamic dc in dcArr)
                        {
                            Assert.NotNull(dc.cycle);
                            Assert.NotNull(dc.expectedReward);
                            Assert.NotNull(dc.status);
                            Assert.Contains((string)dc.status, validCycleStatuses);
                        }
                    }

                    var stakingByCycle = v.stakingRewardsByCycle;
                    Assert.NotNull(stakingByCycle);
                    if (stakingByCycle is DJsonArray scArr && scArr.Count > 0)
                    {
                        dynamic sc = scArr.First();
                        Assert.NotNull(sc.cycle);
                        Assert.NotNull(sc.amount);
                    }
                }
            }
        }
    }
}
