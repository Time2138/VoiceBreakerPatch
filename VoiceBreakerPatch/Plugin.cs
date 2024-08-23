using Exiled.API.Features;
using System;
using HarmonyLib;

namespace VoiceBreakerPatch;

public class Plugin : Plugin<Config, Translation>
{
    public override string Prefix => "VoiceBreakerPatch";
    public override string Name => Prefix;
    public override string Author => "Timersky";
    public override Version Version => new(1, 0, 0);
    
    public static Plugin Instance;
    
    static readonly Harmony Patch = new("VoiceBreakerPatch");

    public override void OnEnabled()
    {
        Instance = this;
        
        Patch.PatchAll();
        
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Patch.UnpatchAll();
        
        Instance = null;
        
        base.OnDisabled();
    }
}