using Microsoft.Extensions.Logging;
using Mvkt.Api.Models;
using Mvkt.Api.Repositories;
using Mvkt.Api.Services;
using Mvkt.Api.Services.Cache;

namespace Mvkt.Api.Services.Delegation
{
    /// <summary>
    /// Service for building comprehensive delegation and staking information for an account
    /// Consolidates data from multiple endpoints (account, rewards, staking, transactions, staking updates)
    /// </summary>
    public class AccountDelegationInfoService
    {
        readonly AccountRepository Accounts;
        readonly RewardsRepository Rewards;
        readonly StakingRepository Staking;
        readonly OperationRepository Operations;
        readonly VotingRepository Voting;
        readonly ILogger<AccountDelegationInfoService> Logger;
        readonly AccountsCache AccountsCache;
        readonly ProtocolsCache Protocols;
        readonly TimeCache Times;
        readonly DelegationConfig DelegationConfig;

        public AccountDelegationInfoService(
            AccountRepository accounts,
            RewardsRepository rewards,
            StakingRepository staking,
            OperationRepository operations,
            VotingRepository voting,
            ILogger<AccountDelegationInfoService> logger,
            AccountsCache accountsCache,
            ProtocolsCache protocols,
            TimeCache times,
            DelegationConfig delegationConfig)
        {
            Accounts = accounts;
            Rewards = rewards;
            Staking = staking;
            Operations = operations;
            Voting = voting;
            Logger = logger;
            AccountsCache = accountsCache;
            Protocols = protocols;
            Times = times;
            DelegationConfig = delegationConfig ?? new DelegationConfig();
        }

        /// <summary>
        /// Get complete delegation info for an account
        /// </summary>
        public async Task<DelegationInfo> GetDelegationInfoAsync(string address, bool legacy = true)
        {
            Account account;
            try
            {
                account = await Accounts.Get(address, legacy);
            }
            catch (Exception ex) when (ex.Message?.Contains("Invalid raw account type") == true)
            {
                return BuildEmptyDelegationInfo();
            }

            if (account == null || account is EmptyAccount)
                return BuildEmptyDelegationInfo();

            int accountId = 0;
            bool isDelegating = false;
            ValidatorInfo delegatedValidator = null;
            long delegatedBalance = 0;
            DateTime? delegationTime = null;
            long stakedBalance = 0;
            long accountBalance = 0;

            if (account is User user)
            {
                accountId = user.Id;
                isDelegating = user.Delegate != null;
                delegatedValidator = user.Delegate != null 
                    ? new ValidatorInfo { Address = user.Delegate.Address, Alias = user.Delegate.Alias }
                    : null;
                delegatedBalance = user.Balance;
                delegationTime = user.DelegationTime;
                stakedBalance = user.StakedBalance;
                accountBalance = user.Balance;
            }
            else if (account is Models.Delegate delegateAccount)
            {
                accountId = delegateAccount.Id;
                stakedBalance = delegateAccount.StakedBalance;
                accountBalance = delegateAccount.Balance;
                isDelegating = false;
            }
            else if (account is Contract contract)
            {
                accountId = contract.Id;
                isDelegating = contract.Delegate != null;
                delegatedValidator = contract.Delegate != null
                    ? new ValidatorInfo { Address = contract.Delegate.Address, Alias = contract.Delegate.Alias }
                    : null;
                delegatedBalance = contract.Balance;
                delegationTime = contract.DelegationTime;
                accountBalance = contract.Balance;
            }
            else
            {
                // Rollup, SmartRollup, Ghost — no delegation/staking in model
                accountId = 0;
                accountBalance = 0;
            }

            if (accountId == 0)
                return BuildEmptyDelegationInfo();

            // Optimized
            var stakingInfo = await GetStakingInfoAsync(accountId);

            // Optimized
            var delegatorRewards = await GetDelegatorRewardsAsync(address);

            var actualRewards = await CalculateActualRewardsAsync(accountId, delegatorRewards, stakingInfo);

            var summary = BuildDelegationSummary(
                account,
                isDelegating,
                delegatedValidator,
                delegatedBalance,
                delegationTime,
                stakingInfo,
                delegatorRewards,
                actualRewards,
                accountBalance);

            var (estimatedNextPayout, avgPayoutInterval) = EstimateNextPayout(actualRewards);

            var lastVote = await GetLastVoteAsync(accountId);

            return new DelegationInfo
            {
                Summary = summary,
                ActualRewards = actualRewards,
                EstimatedNextPayout = estimatedNextPayout,
                AvgPayoutIntervalDays = avgPayoutInterval,
                LastVote = lastVote
            };
        }

        /// <summary>
        /// Get staking information for an account (by internal id). Call only when accountId is known to exist.
        /// Uses aggregated query (staked balance, baker address and alias in one DB round-trip).
        /// </summary>
        private async Task<List<StakerData>> GetStakingInfoAsync(int accountId)
        {
            try
            {
                var list = await Staking.GetStakerBalancesByBakerAsync(accountId);
                return list.ToList();
            }
            catch
            {
                return new List<StakerData>();
            }
        }

        /// <summary>
        /// Get delegator rewards for an address
        /// </summary>
        private async Task<List<DelegatorRewards>> GetDelegatorRewardsAsync(string address)
        {
            try
            {
                var rewards = await Rewards.GetDelegatorRewards(
                    address,
                    cycle: null,
                    sort: new SortParameter { Asc = "cycle" },
                    offset: new OffsetParameter { El = 0 },
                    limit: 10000,
                    quote: Symbols.None
                );

                return rewards.ToList();
            }
            catch
            {
                return new List<DelegatorRewards>();
            }
        }

        /// <summary>
        /// Calculate actual rewards from delegation payout aggregates and staking restake events.
        /// Loads payouts and restake levels from DB, then aggregates expected/actual per validator and returns ActualRewards.
        /// </summary>
        private async Task<ActualRewards> CalculateActualRewardsAsync(
            int accountId,
            List<DelegatorRewards> delegatorRewards,
            List<StakerData> stakingInfo)
        {
            var validatorAddresses = delegatorRewards
                .Select(r => r.Baker?.Address)
                .Where(a => a != null)
                .ToHashSet();
            var validatorIds = validatorAddresses
                .Select(addr => AccountsCache.Get(addr))
                .Where(acc => acc != null)
                .Select(acc => acc.Id)
                .ToList();

            var payoutRows = (await Operations.GetDelegationPayoutsToAccountAsync(accountId, validatorIds)).ToList();

            // Include payouts from configured alternative payout addresses (validator's payout wallet)
            var payoutAddressToValidator = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var payoutAccountIds = new List<int>();
            if (DelegationConfig.ValidatorPayoutAddresses != null)
            {
                foreach (var validatorAddress in validatorAddresses)
                {
                    if (string.IsNullOrEmpty(validatorAddress)) continue;
                    if (!DelegationConfig.ValidatorPayoutAddresses.TryGetValue(validatorAddress, out var payoutAddresses) || payoutAddresses == null)
                        continue;
                    foreach (var payoutAddr in payoutAddresses)
                    {
                        if (string.IsNullOrEmpty(payoutAddr)) continue;
                        var acc = AccountsCache.Get(payoutAddr);
                        if (acc != null)
                        {
                            payoutAddressToValidator[payoutAddr] = validatorAddress;
                            payoutAccountIds.Add(acc.Id);
                        }
                    }
                }
            }

            if (payoutAccountIds.Count > 0)
            {
                var payoutWalletRows = await Operations.GetDelegationPayoutsToAccountAsync(accountId, payoutAccountIds);
                foreach (var row in payoutWalletRows)
                {
                    var senderAddr = row.SenderAddress ?? "";
                    if (!payoutAddressToValidator.TryGetValue(senderAddr, out var validatorAddr)) continue;
                    var validatorAlias = AccountsCache.Get(validatorAddr)?.Alias ?? row.SenderAlias;
                    payoutRows.Add((validatorAddr, validatorAlias, row.Amount, row.Count, row.MinLevel, row.MaxLevel));
                }
            }

            var delegationPayoutsByValidator = payoutRows
                .GroupBy(r => r.SenderAddress ?? "")
                .ToDictionary(g => g.Key, g =>
                {
                    var first = g.First();
                    return new ValidatorPayoutData
                    {
                        Amount = g.Sum(r => r.Amount),
                        Count = g.Sum(r => r.Count),
                        Alias = first.SenderAlias
                    };
                });
            var delegationPayouts = payoutRows.Sum(r => r.Amount);
            var delegationPayoutCount = payoutRows.Sum(r => r.Count);
            var minPayoutLevel = payoutRows.Count > 0 ? payoutRows.Min(r => r.MinLevel) : (int?)null;
            var maxPayoutLevel = payoutRows.Count > 0 ? payoutRows.Max(r => r.MaxLevel) : (int?)null;

            var restakeLevels = await Staking.GetStakerRestakeLevelsAsync(accountId);
            var restakeTimestamps = restakeLevels.Select(l => Times[l]).ToList();
            var allDates = new List<DateTime>(restakeTimestamps);
            if (minPayoutLevel.HasValue) allDates.Add(Times[minPayoutLevel.Value]);
            if (maxPayoutLevel.HasValue && maxPayoutLevel != minPayoutLevel) allDates.Add(Times[maxPayoutLevel.Value]);
            allDates.Sort();

            var aggregated = AggregateRewardsFromDelegatorRewards(delegatorRewards);
            var allStakingRewardEvents = aggregated.AllStakingRewardEvents.OrderByDescending(e => e.Cycle).ToList();

            var allValidators = new HashSet<string>(
                aggregated.ExpectedByValidator.Keys
                    .Concat(delegationPayoutsByValidator.Keys)
                    .Concat(aggregated.StakingByValidator.Keys));
            var byValidator = new List<ValidatorRewardTracking>(allValidators.Count);

            foreach (var address in allValidators)
            {
                aggregated.ExpectedByValidator.TryGetValue(address, out var cycleInfo);
                delegationPayoutsByValidator.TryGetValue(address, out var actualDelegation);
                aggregated.StakingByValidator.TryGetValue(address, out var actualStaking);

                var actualDelegationPayouts = actualDelegation?.Amount ?? 0;
                var actualStakingRestaked = actualStaking?.Amount ?? 0;
                var actualTotalReceived = actualDelegationPayouts + actualStakingRestaked;
                var expectedDelegationRewards = cycleInfo?.DelegationRewards ?? 0;
                var expectedStakingRewards = actualStakingRestaked;
                var expectedTotalGross = expectedStakingRewards + expectedDelegationRewards;
                var pendingDelegation = Math.Max(0, expectedDelegationRewards - actualDelegationPayouts);
                var pendingTotal = pendingDelegation + 0L;

                var (paymentStatus, paymentPercentage) = GetPaymentStatus(
                    actualDelegationPayouts, expectedDelegationRewards, actualStakingRestaked);

                var cycles = cycleInfo?.Cycles ?? new List<int>();
                var alias = cycleInfo?.Alias ?? actualDelegation?.Alias ?? actualStaking?.Alias;
                var stakingByCycle = actualStaking?.ByCycle ?? new List<CycleStakingReward>();
                var delegationCycles = cycleInfo?.DelegationByCycle ?? new List<CycleDelegationData>();
                var sortedDelegationCycles = delegationCycles.Count > 0
                    ? delegationCycles.OrderBy(d => d.Cycle).ToList()
                    : delegationCycles;
                var delegationByCycleStatus = BuildDelegationByCycleStatus(
                    sortedDelegationCycles, actualDelegationPayouts, address, alias);

                var sortedStakingByCycle = stakingByCycle.Count > 0
                    ? stakingByCycle.OrderByDescending(s => s.Cycle).ToList()
                    : stakingByCycle;

                byValidator.Add(new ValidatorRewardTracking
                {
                    Address = address,
                    Alias = alias,
                    ExpectedStakingRewards = expectedStakingRewards,
                    ExpectedDelegationRewards = expectedDelegationRewards,
                    ExpectedTotalGross = expectedTotalGross,
                    ActualDelegationPayouts = actualDelegationPayouts,
                    ActualStakingRestaked = actualStakingRestaked,
                    ActualTotalReceived = actualTotalReceived,
                    PendingDelegation = pendingDelegation,
                    PendingStaking = 0L,
                    PendingTotal = pendingTotal,
                    PaymentStatus = paymentStatus,
                    PaymentPercentage = paymentPercentage,
                    CycleCount = cycles.Count,
                    FirstCycle = cycles.Count > 0 ? cycles.Min() : 0,
                    LastCycle = cycles.Count > 0 ? cycles.Max() : 0,
                    StakingRewardsByCycle = sortedStakingByCycle,
                    StakingRestakeEventCount = actualStaking?.Count ?? 0,
                    DelegationByCycle = delegationByCycleStatus
                });
            }

            byValidator.Sort((a, b) => b.ActualTotalReceived.CompareTo(a.ActualTotalReceived));

            DateTime? firstRewardDate;
            DateTime? lastRewardDate;
            if (allDates.Count > 0)
            {
                firstRewardDate = allDates[0];
                lastRewardDate = allDates[allDates.Count - 1];
            }
            else
            {
                firstRewardDate = null;
                lastRewardDate = null;
            }

            long totalExpectedDelegation = aggregated.ExpectedByValidator.Values.Sum(v => v.DelegationRewards);
            long totalActualRewards = delegationPayouts + aggregated.StakingRestakedTotal;
            long expectedTotalRewards = aggregated.StakingRestakedTotal + totalExpectedDelegation;
            long totalPendingDelegation = Math.Max(0, totalExpectedDelegation - delegationPayouts);

            return new ActualRewards
            {
                TotalActualRewards = totalActualRewards,
                DelegationPayouts = delegationPayouts,
                DelegationPayoutCount = delegationPayoutCount,
                StakingRewardsRestaked = aggregated.StakingRestakedTotal,
                StakingRestakeCount = aggregated.StakingRestakeCount,
                ExpectedDelegationRewards = totalExpectedDelegation,
                ExpectedStakingRewards = aggregated.StakingRestakedTotal,
                ExpectedTotalRewards = expectedTotalRewards,
                PendingDelegation = totalPendingDelegation,
                PendingStaking = 0,
                PendingTotal = totalPendingDelegation,
                ByValidator = byValidator,
                AllStakingRewardEvents = allStakingRewardEvents,
                FirstRewardDate = firstRewardDate,
                LastRewardDate = lastRewardDate
            };
        }

        /// <summary>
        /// Aggregates staking rewards and expected (delegation + staking) rewards per validator from per-cycle delegator rewards.
        /// </summary>
        private static AggregatedRewardsFromDelegators AggregateRewardsFromDelegatorRewards(List<DelegatorRewards> delegatorRewards)
        {
            var stakingRewardsByValidator = new Dictionary<string, ValidatorStakingData>();
            long stakingRewardsRestaked = 0;
            int stakingRestakeCount = 0;
            var allStakingRewardEvents = new List<StakingRewardEvent>();
            var expectedByValidator = new Dictionary<string, ValidatorExpectedData>();

            foreach (var reward in delegatorRewards)
            {
                var bakerAddress = reward.Baker?.Address ?? "";
                if (string.IsNullOrEmpty(bakerAddress)) continue;

                var bakerTotalSharedRewards = reward.BlockRewardsStakedShared + reward.EndorsementRewardsStakedShared;
                var userStakedBalance = reward.StakedBalance;
                var externalStakedBalance = reward.ExternalStakedBalance;

                long stakingReward = 0;
                if (externalStakedBalance > 0 && userStakedBalance > 0)
                    stakingReward = (long)((double)userStakedBalance / externalStakedBalance * bakerTotalSharedRewards);

                if (stakingReward > 0)
                {
                    if (!stakingRewardsByValidator.TryGetValue(bakerAddress, out var stakingData))
                    {
                        stakingData = new ValidatorStakingData
                        {
                            Amount = 0,
                            Count = 0,
                            Alias = reward.Baker?.Name,
                            ByCycle = new List<CycleStakingReward>()
                        };
                        stakingRewardsByValidator[bakerAddress] = stakingData;
                    }
                    stakingData.Amount += stakingReward;
                    stakingData.Count += 1;
                    stakingData.ByCycle.Add(new CycleStakingReward { Cycle = reward.Cycle, Amount = stakingReward, Timestamp = null });
                    stakingRewardsRestaked += stakingReward;
                    stakingRestakeCount += 1;
                    allStakingRewardEvents.Add(new StakingRewardEvent
                    {
                        Cycle = reward.Cycle,
                        Amount = stakingReward,
                        Timestamp = null,
                        ValidatorAddress = bakerAddress,
                        ValidatorAlias = reward.Baker?.Name
                    });
                }

                var validatorDelegationRewards = reward.BlockRewardsDelegated + reward.EndorsementRewardsDelegated;
                var userDelegatedBalance = reward.DelegatedBalance;
                var validatorStakingBalance = reward.StakingBalance;
                long userDelegationReward = 0;
                if (validatorStakingBalance > 0 && userDelegatedBalance > 0)
                    userDelegationReward = (long)((double)userDelegatedBalance / validatorStakingBalance * validatorDelegationRewards);
                long userStakingReward = 0;
                if (externalStakedBalance > 0 && userStakedBalance > 0)
                    userStakingReward = (long)((double)userStakedBalance / externalStakedBalance * bakerTotalSharedRewards);

                if (!expectedByValidator.TryGetValue(bakerAddress, out var expectedData))
                {
                    expectedData = new ValidatorExpectedData
                    {
                        StakingRewards = 0,
                        DelegationRewards = 0,
                        Alias = reward.Baker?.Name,
                        Cycles = new List<int>(),
                        DelegationByCycle = new List<CycleDelegationData>()
                    };
                    expectedByValidator[bakerAddress] = expectedData;
                }
                expectedData.DelegationRewards += userDelegationReward;
                expectedData.StakingRewards += userStakingReward;
                expectedData.Cycles.Add(reward.Cycle);
                if (userDelegationReward > 0)
                    expectedData.DelegationByCycle.Add(new CycleDelegationData { Cycle = reward.Cycle, Amount = userDelegationReward });
            }

            return new AggregatedRewardsFromDelegators
            {
                StakingByValidator = stakingRewardsByValidator,
                StakingRestakedTotal = stakingRewardsRestaked,
                StakingRestakeCount = stakingRestakeCount,
                AllStakingRewardEvents = allStakingRewardEvents,
                ExpectedByValidator = expectedByValidator
            };
        }

        /// <summary>
        /// Estimate next payout date and average interval in days based on aggregated rewards timeline.
        /// Uses first/last reward dates and total count of payout + restake events.
        /// </summary>
        private (DateTime?, double?) EstimateNextPayout(ActualRewards actualRewards)
        {
            if (actualRewards == null)
            {
                Logger.LogInformation("EstimateNextPayout: actualRewards is null");
                return (null, null);
            }

            var first = actualRewards.FirstRewardDate;
            var last = actualRewards.LastRewardDate;
            if (!first.HasValue || !last.HasValue)
            {
                Logger.LogInformation(
                    "EstimateNextPayout: missing reward dates. First={First}, Last={Last}",
                    first, last);
                return (null, null);
            }

            var eventCount = actualRewards.DelegationPayoutCount + actualRewards.StakingRestakeCount;
            if (eventCount < 2)
            {
                Logger.LogInformation(
                    "EstimateNextPayout: insufficient events. DelegationPayoutCount={DelegationPayoutCount}, StakingRestakeCount={StakingRestakeCount}",
                    actualRewards.DelegationPayoutCount,
                    actualRewards.StakingRestakeCount);
                return (null, null);
            }

            var totalDays = (last.Value - first.Value).TotalDays;
            if (totalDays <= 0)
            {
                Logger.LogInformation(
                    "EstimateNextPayout: non-positive totalDays between first and last reward. First={First}, Last={Last}, TotalDays={TotalDays}",
                    first, last, totalDays);
                return (null, null);
            }

            var avgInterval = totalDays / (eventCount - 1);
            if (avgInterval <= 0)
            {
                Logger.LogInformation(
                    "EstimateNextPayout: non-positive avgInterval. TotalDays={TotalDays}, EventCount={EventCount}, AvgInterval={AvgInterval}",
                    totalDays, eventCount, avgInterval);
                return (null, null);
            }

            var estimatedNext = last.Value.AddDays(avgInterval);
            Logger.LogInformation(
                "EstimateNextPayout: success. First={First}, Last={Last}, EventCount={EventCount}, TotalDays={TotalDays}, AvgInterval={AvgInterval}, EstimatedNext={EstimatedNext}",
                first, last, eventCount, totalDays, avgInterval, estimatedNext);
            return (estimatedNext, avgInterval);
        }

        /// <summary>
        /// Computes payment status and percentage for a validator based on expected vs actual payouts.
        /// </summary>
        private static (string Status, double Percentage) GetPaymentStatus(
            long actualDelegationPayouts,
            long expectedDelegationRewards,
            long actualStakingRestaked)
        {
            if (expectedDelegationRewards > 0)
            {
                var percentage = Math.Min(100, (double)actualDelegationPayouts / expectedDelegationRewards * 100);
                string status = actualDelegationPayouts >= expectedDelegationRewards * 0.99
                    ? "fully_paid"
                    : actualDelegationPayouts > expectedDelegationRewards
                        ? "overpaid"
                        : actualDelegationPayouts > 0
                            ? "underpaid"
                            : "pending";
                if (status == "overpaid")
                    percentage = (double)actualDelegationPayouts / expectedDelegationRewards * 100;
                return (status, percentage);
            }
            if (actualDelegationPayouts > 0 || actualStakingRestaked > 0)
                return ("fully_paid", 100);
            return ("pending", 0);
        }

        /// <summary>
        /// Builds per-cycle payment status for delegation rewards (paid / underpaid / pending / overpaid).
        /// </summary>
        private static List<CyclePaymentStatus> BuildDelegationByCycleStatus(
            List<CycleDelegationData> sortedDelegationCycles,
            long actualDelegationPayouts,
            string validatorAddress,
            string validatorAlias)
        {
            var result = new List<CyclePaymentStatus>(sortedDelegationCycles.Count);
            long remainingPayout = actualDelegationPayouts;

            foreach (var cycleData in sortedDelegationCycles)
            {
                string cycleStatus = remainingPayout >= cycleData.Amount * 0.99
                    ? "paid"
                    : remainingPayout > 0
                        ? "underpaid"
                        : "pending";
                if (cycleStatus == "paid")
                    remainingPayout -= cycleData.Amount;
                else if (remainingPayout > 0)
                    remainingPayout = 0;

                result.Add(new CyclePaymentStatus
                {
                    Cycle = cycleData.Cycle,
                    ExpectedReward = cycleData.Amount,
                    Status = cycleStatus,
                    ValidatorAddress = validatorAddress,
                    ValidatorAlias = validatorAlias
                });
            }

            if (remainingPayout > 0 && result.Count > 0)
                result[result.Count - 1].Status = "overpaid";

            result.Sort((a, b) => b.Cycle.CompareTo(a.Cycle));
            return result;
        }

        /// <summary>
        /// Build empty delegation info for non-existent or unresolved accounts
        /// </summary>
        private static DelegationInfo BuildEmptyDelegationInfo()
        {
            return new DelegationInfo
            {
                Summary = new DelegationSummary
                {
                    IsDelegating = false,
                    DelegatedBalance = 0,
                    IsStaking = false,
                    StakedValidators = new List<StakedValidatorInfo>(),
                    TotalStakedBalance = 0,
                    TotalRewardsEarned = 0,
                    RewardsByValidator = new List<ValidatorRewardSummary>(),
                    RewardsByCycle = new List<CycleRewardSummary>(),
                    StakingPercentage = 0,
                    DelegationPercentage = 0,
                    TotalValidatorsUsed = 0,
                    CurrentValidatorCount = 0,
                    PastValidatorCount = 0,
                    TotalCyclesWithRewards = 0
                },
                ActualRewards = new ActualRewards
                {
                    ByValidator = new List<ValidatorRewardTracking>(),
                    AllStakingRewardEvents = new List<StakingRewardEvent>()
                },
                EstimatedNextPayout = null,
                AvgPayoutIntervalDays = null,
                LastVote = null
            };
        }

        /// <summary>
        /// Build delegation summary
        /// </summary>
        private DelegationSummary BuildDelegationSummary(
            Account account,
            bool isDelegating,
            ValidatorInfo delegatedValidator,
            long delegatedBalance,
            DateTime? delegationTime,
            List<StakerData> stakingInfo,
            List<DelegatorRewards> delegatorRewards,
            ActualRewards actualRewards,
            long accountBalance)
        {
            var isStaking = stakingInfo.Any();
            var totalStakedBalance = stakingInfo.Sum(s => s.StakedBalance);

            var stakedValidators = stakingInfo.Select(s => new StakedValidatorInfo
            {
                Baker = new ValidatorInfo { Address = s.BakerAddress, Alias = s.BakerAlias },
                StakedBalance = s.StakedBalance
            }).ToList();

            var rewardsByValidator = actualRewards.ByValidator.Select(v => new ValidatorRewardSummary
            {
                Validator = new ValidatorInfo { Address = v.Address, Alias = v.Alias },
                TotalRewards = v.ActualTotalReceived,
                CycleCount = v.CycleCount,
                FirstCycle = v.FirstCycle,
                LastCycle = v.LastCycle,
                TotalStakingRewards = v.ActualStakingRestaked,
                TotalDelegationRewards = v.ActualDelegationPayouts,
                IsCurrentValidator = (delegatedValidator?.Address == v.Address) || stakingInfo.Any(s => s.BakerAddress == v.Address)
            }).ToList();

            var rewardsByCycle = delegatorRewards.Select(r =>
            {
                var bakerTotalSharedRewards = r.BlockRewardsStakedShared + r.EndorsementRewardsStakedShared;
                var userStakedBalance = r.StakedBalance;
                var externalStakedBalance = r.ExternalStakedBalance;
                long stakingReward = 0;
                if (externalStakedBalance > 0 && userStakedBalance > 0)
                {
                    stakingReward = (long)((double)userStakedBalance / externalStakedBalance * bakerTotalSharedRewards);
                }

                var validatorDelegationRewards = r.BlockRewardsDelegated + r.EndorsementRewardsDelegated;
                var userDelegatedBalance = r.DelegatedBalance;
                var validatorStakingBalance = r.StakingBalance;
                long delegationReward = 0;
                if (validatorStakingBalance > 0 && userDelegatedBalance > 0)
                {
                    delegationReward = (long)((double)userDelegatedBalance / validatorStakingBalance * validatorDelegationRewards);
                }

                return new CycleRewardSummary
                {
                    Cycle = r.Cycle,
                    Rewards = stakingReward + delegationReward,
                    DelegatedBalance = r.DelegatedBalance,
                    StakedBalance = r.StakedBalance,
                    Validator = new ValidatorInfo { Address = r.Baker?.Address, Alias = r.Baker?.Name },
                    StakingRewards = stakingReward,
                    DelegationRewards = delegationReward
                };
            }).ToList();

            double stakingPercentage = accountBalance > 0 ? (double)totalStakedBalance / accountBalance * 100 : 0;
            double delegationPercentage = accountBalance > 0 && isDelegating ? (double)delegatedBalance / accountBalance * 100 : 0;

            var allValidators = new HashSet<string>();
            var currentValidators = new HashSet<string>();
            if (isDelegating && delegatedValidator != null)
            {
                allValidators.Add(delegatedValidator.Address);
                currentValidators.Add(delegatedValidator.Address);
            }
            foreach (var staker in stakingInfo)
            {
                allValidators.Add(staker.BakerAddress);
                currentValidators.Add(staker.BakerAddress);
            }
            foreach (var reward in delegatorRewards)
            {
                if (reward.Baker != null)
                    allValidators.Add(reward.Baker.Address);
            }

            var cycles = delegatorRewards.Select(r => r.Cycle).ToList();
            int? firstRewardCycle = cycles.Any() ? cycles.Min() : null;
            int? lastRewardCycle = cycles.Any() ? cycles.Max() : null;
            int totalCyclesWithRewards = cycles.Distinct().Count();

            return new DelegationSummary
            {
                IsDelegating = isDelegating,
                DelegatedValidator = delegatedValidator,
                DelegatedBalance = delegatedBalance,
                DelegationTime = delegationTime,
                IsStaking = isStaking,
                StakedValidators = stakedValidators,
                TotalStakedBalance = totalStakedBalance,
                TotalRewardsEarned = actualRewards.TotalActualRewards,
                RewardsByValidator = rewardsByValidator,
                RewardsByCycle = rewardsByCycle,
                StakingPercentage = stakingPercentage,
                DelegationPercentage = delegationPercentage,
                TotalValidatorsUsed = allValidators.Count,
                CurrentValidatorCount = currentValidators.Count,
                PastValidatorCount = allValidators.Count - currentValidators.Count,
                FirstRewardCycle = firstRewardCycle,
                LastRewardCycle = lastRewardCycle,
                TotalCyclesWithRewards = totalCyclesWithRewards
            };
        }

        /// <summary>
        /// Get last vote information. Call only when accountId is known to exist.
        /// </summary>
        private async Task<LastVoteInfo> GetLastVoteAsync(int accountId)
        {
            try
            {
                var ballots = await Operations.GetBallots(
                    sender: new AccountParameter { Eq = accountId },
                    level: null,
                    timestamp: null,
                    epoch: null,
                    period: null,
                    proposal: null,
                    vote: null,
                    sort: new SortParameter { Desc = "id" },
                    offset: new OffsetParameter { El = 0 },
                    limit: 1,
                    quote: Symbols.None
                );

                var ballot = ballots.FirstOrDefault();
                if (ballot == null)
                    return null;

                if (ballot is Models.BallotOperation ballotOp)
                {
                    return new LastVoteInfo
                    {
                        Proposal = ballotOp.Proposal?.Hash,
                        Vote = ballotOp.Vote,
                        Period = ballotOp.Period?.Index ?? 0,
                        Timestamp = ballotOp.Timestamp
                    };
                }
            }
            catch
            {
                // Ignore errors, last vote is optional
            }

            return null;
        }

        /// <summary>
        /// Result of aggregating staking and expected rewards from delegator rewards per cycle.
        /// </summary>
        private class AggregatedRewardsFromDelegators
        {
            public Dictionary<string, ValidatorStakingData> StakingByValidator { get; set; }
            public long StakingRestakedTotal { get; set; }
            public int StakingRestakeCount { get; set; }
            public List<StakingRewardEvent> AllStakingRewardEvents { get; set; }
            public Dictionary<string, ValidatorExpectedData> ExpectedByValidator { get; set; }
        }

        private class ValidatorPayoutData
        {
            public long Amount { get; set; }
            public int Count { get; set; }
            public string Alias { get; set; }
        }

        private class ValidatorStakingData
        {
            public long Amount { get; set; }
            public int Count { get; set; }
            public string Alias { get; set; }
            public List<CycleStakingReward> ByCycle { get; set; }
        }

        private class ValidatorExpectedData
        {
            public long StakingRewards { get; set; }
            public long DelegationRewards { get; set; }
            public string Alias { get; set; }
            public List<int> Cycles { get; set; }
            public List<CycleDelegationData> DelegationByCycle { get; set; }
        }

        private class CycleDelegationData
        {
            public int Cycle { get; set; }
            public long Amount { get; set; }
        }
    }
}
