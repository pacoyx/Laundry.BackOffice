using System.Text.Json.Serialization;

public record AccessScheduleWindowResponse(
    int Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);

public record UserAccessScheduleResponse(
    int UserId,
    string UserName,
    List<AccessScheduleWindowResponse> Windows
);
