using System;
using System.Collections.Generic;
using System.Text;


namespace LMS.Shared.DTOs.FeedbackDtos;

public sealed record FeedbackDto(
    int Id,
    int SubmissionId,
    string Text,
    DateTime CreatedAt,
    string TeacherId,
    string TeacherName);

