using Exiled.API.Interfaces;

namespace VoiceBreakerPatch;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    public bool BanForVoiceExploit { get; set; } = true;
    public string VoiceExploitBanReason { get; set; } = "Cheats | This ban was issued automatically";
 }