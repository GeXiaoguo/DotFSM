namespace IssueTracker;

public static class EnumExtensions
{
    public static T? ToEnum<T>(this string enumName) where T : struct => Enum.TryParse<T>(enumName, true, out T result) ? result : null;
}
