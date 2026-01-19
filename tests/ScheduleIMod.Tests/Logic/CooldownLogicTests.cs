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
    }
}
