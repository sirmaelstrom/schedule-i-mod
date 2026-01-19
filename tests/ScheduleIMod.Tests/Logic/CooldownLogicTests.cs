using FluentAssertions;
using ScheduleIMod.Logic;
using Xunit;

namespace ScheduleIMod.Tests.Logic
{
    public class CooldownLogicTests
    {
        [Theory]
        [InlineData(0.0f, 0f)]     // No cooldown
        [InlineData(0.5f, 180f)]   // Half cooldown
        [InlineData(1.0f, 360f)]   // Vanilla
        [InlineData(2.0f, 720f)]   // Double
        public void ScaleCooldown_CalculatesCorrectly(float multiplier, float expected)
        {
            var result = CooldownLogic.ScaleCooldown(360f, multiplier);
            result.Should().Be(expected);
        }

        [Fact]
        public void ShouldAllowDeal_BlocksWhenPendingOfferExists()
        {
            var result = CooldownLogic.ShouldAllowDeal(
                timeSinceLastCompleted: 1000f,
                timeSinceLastOffered: 1000f,
                hasPendingOffer: true,
                hasPendingInstantDeal: false,
                cooldownMultiplier: 0.0f,
                out string invalidReason
            );

            result.Should().BeFalse();
            invalidReason.Should().Contain("pending offer");
        }

        [Theory]
        [InlineData(0.0f, 100f, true)]   // 0x multiplier, any time = pass
        [InlineData(1.0f, 359f, false)]  // 1x, just under 360s = fail
        [InlineData(1.0f, 360f, true)]   // 1x, exactly at 360s = pass
        [InlineData(1.0f, 361f, true)]   // 1x, over 360s = pass
        [InlineData(0.5f, 179f, false)]  // 0.5x, under 180s = fail
        [InlineData(0.5f, 180f, true)]   // 0.5x, at 180s = pass
        [InlineData(2.0f, 719f, false)]  // 2.0x, under 720s = fail
        [InlineData(2.0f, 720f, true)]   // 2.0x, at 720s = pass
        public void ShouldAllowDeal_RespectsScaledCooldown_LastCompleted(
            float multiplier,
            float timeSince,
            bool shouldAllow)
        {
            var result = CooldownLogic.ShouldAllowDeal(
                timeSinceLastCompleted: timeSince,
                timeSinceLastOffered: 1000f, // Well past cooldown
                hasPendingOffer: false,
                hasPendingInstantDeal: false,
                cooldownMultiplier: multiplier,
                out _
            );

            result.Should().Be(shouldAllow);
        }

        [Theory]
        [InlineData(0.5f, 179f, false)]  // Just under boundary
        [InlineData(0.5f, 180f, true)]   // At boundary
        public void ShouldAllowDeal_RespectsScaledCooldown_LastOffered(
            float multiplier,
            float timeSince,
            bool shouldAllow)
        {
            var result = CooldownLogic.ShouldAllowDeal(
                timeSinceLastCompleted: 1000f, // Well past cooldown
                timeSinceLastOffered: timeSince,
                hasPendingOffer: false,
                hasPendingInstantDeal: false,
                cooldownMultiplier: multiplier,
                out _
            );

            result.Should().Be(shouldAllow);
        }
    }
}
