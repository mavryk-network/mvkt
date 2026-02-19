using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dynamic.Json;
using Dynamic.Json.Extensions;
using Xunit;

namespace Mvkt.Api.Tests.Api
{
    public class TestDelegationInfoQueries : IClassFixture<SettingsFixture>
    {
        readonly SettingsFixture Settings;
        readonly HttpClient Client;

        public TestDelegationInfoQueries(SettingsFixture settings)
        {
            Settings = settings;
            Client = settings.Client;
        }

        [Fact]
        public async Task TestAccountEndpoint_DoesNotIncludeDelegationInfo()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}");

            Assert.True(res is DJsonObject);
            Assert.Null(res.delegationInfo);
            Assert.NotNull(res.address);
            Assert.NotNull(res.balance);
            Assert.NotNull(res.type);
        }

        [Fact]
        public async Task TestDelegationInfoEndpoint_ReturnsDelegationInfo()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegation-info");

            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.NotNull(res.actualRewards);
        }

        [Fact]
        public async Task TestDelegationInfoStructure()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegation-info");

            Assert.True(res is DJsonObject);

            Assert.NotNull(res.summary);
            Assert.NotNull(res.summary.isDelegating);
            Assert.NotNull(res.summary.isStaking);
            Assert.NotNull(res.summary.totalRewardsEarned);
            Assert.NotNull(res.summary.rewardsByValidator);
            Assert.NotNull(res.summary.rewardsByCycle);

            Assert.NotNull(res.actualRewards);
            Assert.NotNull(res.actualRewards.totalActualRewards);
            Assert.NotNull(res.actualRewards.delegationPayouts);
            Assert.NotNull(res.actualRewards.stakingRewardsRestaked);
            Assert.NotNull(res.actualRewards.expectedTotalRewards);
            Assert.NotNull(res.actualRewards.pendingDelegation);
            Assert.NotNull(res.actualRewards.byValidator);
        }

        [Fact]
        public async Task TestDelegatorAccountDelegationInfo()
        {
            var delegators = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegators?limit=1");

            if (delegators is DJsonArray delegatorsArray && delegatorsArray.Count > 0)
            {
                dynamic delegator = delegatorsArray.First();
                var delegatorAddress = (string)delegator.address;

                dynamic res = await Client.GetJsonAsync($"/v1/accounts/{delegatorAddress}/delegation-info");

                Assert.True(res is DJsonObject);
                Assert.NotNull(res.summary);
                Assert.NotNull(res.actualRewards);
            }
        }

        [Fact]
        public async Task TestNonExistentAccountDelegationInfo_Returns200WithEmptyInfo()
        {
            // Non-existent mv address: API returns 200 with empty delegation info (no 404)
            var response = await Client.GetAsync("/v1/accounts/mv1V4h45W3p4e1sjSBvRkK2uYbvkTnSuHg1c/delegation-info");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            dynamic res = await Client.GetJsonAsync("/v1/accounts/mv1V4h45W3p4e1sjSBvRkK2uYbvkTnSuHg1c/delegation-info");
            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.False((bool)res.summary.isDelegating);
            Assert.False((bool)res.summary.isStaking);
            Assert.NotNull(res.summary.stakedValidators);
            Assert.True(res.summary.stakedValidators is DJsonArray);
            Assert.Equal(0, ((DJsonArray)res.summary.stakedValidators).Count);
            Assert.Equal(0L, (long)res.summary.totalStakedBalance);
            Assert.Equal(0L, (long)res.summary.totalRewardsEarned);
            Assert.NotNull(res.actualRewards);
            Assert.NotNull(res.actualRewards.byValidator);
            Assert.True(res.actualRewards.byValidator is DJsonArray);
            Assert.Equal(0, ((DJsonArray)res.actualRewards.byValidator).Count);
        }

        [Fact]
        public async Task TestDelegationInfoByValidatorTracking()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegation-info");

            Assert.True(res is DJsonObject);
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
            }
        }

        [Fact]
        public async Task TestDelegationInfo_WithLegacyParameter()
        {
            var resLegacyTrue = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegation-info?legacy=true");
            var resLegacyFalse = await Client.GetJsonAsync($"/v1/accounts/{Settings.Baker}/delegation-info?legacy=false");

            Assert.True(resLegacyTrue is DJsonObject);
            Assert.True(resLegacyFalse is DJsonObject);
            Assert.NotNull(resLegacyTrue.summary);
            Assert.NotNull(resLegacyFalse.summary);
            Assert.NotNull(resLegacyTrue.actualRewards);
            Assert.NotNull(resLegacyFalse.actualRewards);
        }

        [Fact]
        public async Task TestContractDelegationInfo()
        {
            dynamic res = await Client.GetJsonAsync($"/v1/accounts/{Settings.Originator}/delegation-info");

            Assert.True(res is DJsonObject);
            Assert.NotNull(res.summary);
            Assert.NotNull(res.actualRewards);
            Assert.NotNull(res.summary.isDelegating);
            Assert.NotNull(res.summary.isStaking);
        }
    }
}
