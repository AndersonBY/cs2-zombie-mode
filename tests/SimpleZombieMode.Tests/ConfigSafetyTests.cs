using SimpleZombieMode.Configs;
using Xunit;

namespace SimpleZombieMode.Tests;

public class ConfigSafetyTests
{
    [Fact]
    public void Normalize_ClampsUnsafeTenantValues()
    {
        var config = new MainConfig
        {
            TimerRound = -1,
            ZombieSpeed = 99,
            ZombieLives = 0,
            MinPlayers = 500
        };

        ConfigSafety.Normalize(config);

        Assert.Equal(30, config.TimerRound);
        Assert.Equal(3.0f, config.ZombieSpeed);
        Assert.Equal(1, config.ZombieLives);
        Assert.Equal(64, config.MinPlayers);
    }
}
