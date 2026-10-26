using System.Text.Json.Serialization;

public record UpdateAccessScheduleWindowRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record UpdateUserAccessScheduleRequest(List<UpdateAccessScheduleWindowRequest> Windows);
