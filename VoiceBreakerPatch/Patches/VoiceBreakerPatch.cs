using System;
using System.Collections.Concurrent;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using UnityEngine;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using Player = Exiled.API.Features.Player;

namespace VoiceBreakerPatch.Patches;

[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
public static class VoiceBreakerPatch
{
    private static readonly OpusDecoder Decoder = new();
    private static readonly OpusEncoder Encoder = new(OpusApplicationType.Voip);

    private static readonly ConcurrentDictionary<Player, int> ExploitMessages = [];

    public static bool Prefix(NetworkConnection conn, ref VoiceMessage msg)
    {
        try
        {
            if (msg.Speaker == null || conn.identity.netId != msg.Speaker.netId) return false;

            var samples = new float[24000];
            int length = Decoder.Decode(msg.Data, msg.DataLength, samples);
                    
            if (length != 480) return false;

            var (min, max) = (samples.Min(), samples.Max());
            float maxVolume = Mathf.Max(max, -min);

            if (maxVolume > 1)
            {
                var speaker = Player.Get(msg.Speaker);
                if (maxVolume > 100 && !speaker.IsTransmitting)
                { 
                    RecordExploitAttempt(speaker);
                }
                        
                ScaleSamples(samples, 1 / maxVolume);
                msg = msg with { Data = Encode(samples) };
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error while checking voice message: {ex}");
            return false;
        }

        return true;
    }

    private static void RecordExploitAttempt(Player player)
    {
        int exploitAttempts = ExploitMessages.AddOrUpdate(player, 1, (_, count) => count + 1);
        if (Plugin.Instance.Config.BanForVoiceExploit && exploitAttempts >= 10)
        {
            Log.Error($"Banning {player.Nickname} for voice exploit");
            player.Ban(DateTime.Now.AddYears(50) - DateTime.Now, Plugin.Instance.Translation.VoiceExploitBanReason);
        }
    }

    private static void ScaleSamples(float[] samples, float scale)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] *= scale;
        }
    }

    private static byte[] Encode(float[] samples)
    {
        var data = new byte[512];
        Encoder.Encode(samples, data);
        return data;
    }
}