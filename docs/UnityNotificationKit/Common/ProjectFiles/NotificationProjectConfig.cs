/// <summary>Settings shared by the standalone local and remote notification modules.</summary>
public static class NotificationProjectConfig
{
    // Replace these three values with identifiers for the receiving game.
    // RemoteChannelId must match the Firebase default channel in AndroidManifest.xml.
    public const string LocalChannelId = "your_game_recall";
    public const string RemoteChannelId = "your_game_push";
    public const string RemoteTopic = "your_game_daily_reminders";

    public const string SmallIconId = "notify_small";

    // Temporarily use 0.02 hours and disable night protection for a local device test.
    public const double FirstRecallDelayHours = 20;
    public const double ThirdRecallDelayHours = 72;
    public const double SeventhRecallDelayHours = 168;
    public const bool AvoidNightHours = true;
}
