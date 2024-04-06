namespace NetBackend.Tools;

public static class ExtractionTools
{
    public static string ExtractUserIdFromTopic(string topic, string topicPrefix)
    {
        string prefix = $"{topicPrefix}-";
        if (topic.StartsWith(prefix))
        {
            return topic[prefix.Length..];
        }

        throw new ArgumentException($"Topic '{topic}' does not start with the expected prefix '{prefix}'.", nameof(topic));
    }

    public static string ExtractValue(string message, string label, string endDelimiter)
    {
        int startIndex = message.IndexOf(label) + label.Length;
        if (startIndex < label.Length) return string.Empty; // Label not found

        int endIndex = endDelimiter != "" ? message.IndexOf(endDelimiter, startIndex) : -1;
        if (endIndex == -1) endIndex = message.Length;

        string value = message.Substring(startIndex, endIndex - startIndex).Trim();

        // Special handling for timestamp to ensure full ISO8601 format is preserved
        if (label.StartsWith("TimeStamp"))
        {
            return value;
        }

        // Adjusting logic to safely handle numeric values including negatives
        return new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray()).Trim();
    }
}