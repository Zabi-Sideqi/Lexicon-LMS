using LMS.Shared.Enums;

namespace LMS.Services.Helpers;

public static class SubmissionStatusCalculator
{
    // Submitted: handed in on or before the deadline.
    // SubmittedLate: handed in after the deadline.
    // Late: the deadline has passed and nothing has been handed in.
    // NotSubmitted: nothing handed in yet, and the deadline has not passed.
    public static SubmissionStatus Calculate(DateTime deadline, DateTime? submittedAt, DateTime now)
    {
        if (submittedAt is not null)
        {
            return submittedAt <= deadline
                ? SubmissionStatus.Submitted
                : SubmissionStatus.SubmittedLate;
        }

        return now > deadline
            ? SubmissionStatus.Late
            : SubmissionStatus.NotSubmitted;
    }
}
