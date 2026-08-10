namespace L2.GameContent;

public static class AssetImportJobValues
{
    public const string SystemTextures = "systextures";
    public const string Textures = "textures";
    public const string Music = "music";
    public const string Sounds = "sounds";
    public const string StaticMeshes = "staticmeshes";
    public const string Levels = "levels";
    public const string LevelPreviews = "levelpreviews";
    public const string Scenes = "scenes";
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string SucceededWithWarnings = "succeeded_with_warnings";
    public const string Failed = "failed";

    public static readonly string[] ActiveStatuses = [Queued, Running];
}
