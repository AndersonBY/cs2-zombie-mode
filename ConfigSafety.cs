using SimpleZombieMode.Configs;

namespace SimpleZombieMode;

public static class ConfigSafety
{
    public static void Normalize(MainConfig config)
    {
        config.TimerStartInfection = Math.Clamp(config.TimerStartInfection, 1, 300);
        config.TimerRound = Math.Clamp(config.TimerRound, 30, 3600);
        config.TimerRestartGame = Math.Clamp(config.TimerRestartGame, 1, 30);
        config.ZombieHealth = Math.Clamp(config.ZombieHealth, 1, 100_000);
        config.ZombieSpeed = Math.Clamp(config.ZombieSpeed, 0.1f, 3.0f);
        config.ZombieLives = Math.Clamp(config.ZombieLives, 1, 100);
        config.ZombieRespawnDelay = Math.Clamp(config.ZombieRespawnDelay, 0.1f, 30.0f);
        config.ZombieHealOnKill = Math.Clamp(config.ZombieHealOnKill, 0, 100_000);
        config.HumanHealth = Math.Clamp(config.HumanHealth, 1, 100_000);
        config.HumanSpeed = Math.Clamp(config.HumanSpeed, 0.1f, 3.0f);
        config.SurvivorHealth = Math.Clamp(config.SurvivorHealth, 1, 100_000);
        config.SurvivorSpeed = Math.Clamp(config.SurvivorSpeed, 0.1f, 3.0f);
        config.MinPlayers = Math.Clamp(config.MinPlayers, 2, 64);
    }
}
