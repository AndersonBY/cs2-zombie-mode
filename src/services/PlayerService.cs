// src/services/PlayerService.cs
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using SimpleZombieMode.Configs;

namespace SimpleZombieMode.Services;

public class PlayerService
{
    private sealed record PlayerSnapshot(CsTeam Team, int Health, float VelocityModifier, IReadOnlyList<string> Weapons);

    private readonly Dictionary<ulong, int> _playerLives = new();
    private readonly Dictionary<ulong, PlayerSnapshot> _playerSnapshots = new();
    private readonly MainConfig _config;
    private readonly Func<RoundPhase> _getRoundPhase;
    private readonly IStringLocalizer _localizer;

    public PlayerService(MainConfig config, Func<RoundPhase> getRoundPhase, IStringLocalizer localizer)
    {
        _config = config;
        _getRoundPhase = getRoundPhase;
        _localizer = localizer;
    }

    // Zombie management -->>
    internal void InfectPlayer(CCSPlayerController player, CCSPlayerController? infectedBy, bool isInitialize = true)
    {
        if (player is null || !player.IsValid) return;

        CapturePlayerState(player);

        if (isInitialize)
        {
            player.ChangeTeam(CsTeam.Terrorist);
            _playerLives[player.SteamID] = _config.ZombieLives;
            if (infectedBy is null) Server.PrintToChatAll(_localizer["szm.zombie.now", _localizer["szm.prefix"], player.PlayerName]);
            else Server.PrintToChatAll(_localizer["szm.zombie.infected_by", _localizer["szm.prefix"], player.PlayerName, infectedBy.PlayerName]);
        }

        player.Respawn();

        // modify player health and speed
        if (player.PlayerPawn.Value is not CCSPlayerPawn pawn)
            return;

        pawn.Health = _config.ZombieHealth;
        pawn.VelocityModifier = _config.ZombieSpeed;

        player.RemoveWeapons();
        player.GiveNamedItem("weapon_knife");
    }

    // Lives management -->>
    internal int RemoveLife(ulong steamId)
    {
        _playerLives[steamId] = Math.Max(_playerLives.GetValueOrDefault(steamId, 0) - 1, 0);
        return _playerLives[steamId];
    }

    internal void ResetLives()
    {
        _playerLives.Clear();
    }

    internal void CapturePlayerState(CCSPlayerController player)
    {
        if (!player.IsValid || player.SteamID == 0 || _playerSnapshots.ContainsKey(player.SteamID)) return;

        var pawn = player.PlayerPawn.Value;
        var weapons = pawn?.WeaponServices?.MyWeapons
            .Select(handle => handle.Value?.DesignerName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .Distinct()
            .ToList() ?? new List<string>();

        _playerSnapshots[player.SteamID] = new PlayerSnapshot(
            player.Team,
            pawn?.Health ?? _config.HumanHealth,
            pawn?.VelocityModifier ?? 1.0f,
            weapons);
    }

    internal void ForgetPlayer(ulong steamId)
    {
        _playerLives.Remove(steamId);
        _playerSnapshots.Remove(steamId);
    }

    internal void RestorePlayers()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid && player.SteamID != 0))
        {
            if (!_playerSnapshots.TryGetValue(player.SteamID, out var snapshot)) continue;

            player.SwitchTeam(snapshot.Team);
            Server.NextFrame(() =>
            {
                if (!player.IsValid) return;

                player.Respawn();
                var pawn = player.PlayerPawn.Value;
                if (pawn is null) return;

                pawn.Health = snapshot.Health;
                pawn.VelocityModifier = snapshot.VelocityModifier;
                player.RemoveWeapons();
                foreach (var weapon in snapshot.Weapons)
                    player.GiveNamedItem(weapon);
            });
        }

        _playerSnapshots.Clear();
    }

    // Events -->>
    internal void OnItemPickup(CCSPlayerController? player, string itemName)
    {
        if (player is null || !player.IsValid) return;

        if (player.Team is CsTeam.Terrorist && _getRoundPhase() is RoundPhase.Active)
        {
            if (itemName is "knife") return;

            Server.NextFrame(() =>
            {
                player.RemoveWeapons();
                player.GiveNamedItem("weapon_knife");
            });
        }
    }
}
