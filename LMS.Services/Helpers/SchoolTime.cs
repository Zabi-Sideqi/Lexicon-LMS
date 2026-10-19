namespace LMS.Services;

// Activity times (such as an assignment deadline) are saved exactly as the teacher typed them,
// which is the school's local time. Timestamps made by the system (SubmittedAt) are saved in UTC.
// To compare them correctly, convert the UTC values to school time first.
public static class SchoolTime
{
    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public static DateTime FromUtc(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            Zone);
}
