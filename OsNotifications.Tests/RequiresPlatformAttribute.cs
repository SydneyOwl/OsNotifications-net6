using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresPlatformAttribute : FactAttribute
{
    public RequiresPlatformAttribute(string platform)
    {
        bool isMatch = platform.ToLowerInvariant() switch
        {
            "linux" => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux),
            "osx" or "macos" => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX),
            "windows" => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows),
            _ => false
        };

        if (!isMatch)
            Skip = $"Test requires {platform}";
    }
}