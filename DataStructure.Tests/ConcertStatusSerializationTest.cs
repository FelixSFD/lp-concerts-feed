using System;
using System.Text.Json;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Tours;
using Xunit;

namespace DataStructure.Tests;

public class ConcertStatusSerializationTest
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Theory]
    [InlineData(ConcertDto.ConcertStatusValue.Planned, "Planned")]
    [InlineData(ConcertDto.ConcertStatusValue.Running, "Running")]
    [InlineData(ConcertDto.ConcertStatusValue.Past, "Past")]
    [InlineData(ConcertDto.ConcertStatusValue.Cancelled, "Cancelled")]
    public void RawConcertDto_SerializesStatusAsString(ConcertDto.ConcertStatusValue status, string expectedStatusString)
    {
        var rawConcert = new RawConcertDto
        {
            Id = "c123",
            Status = status
        };

        var json = JsonSerializer.Serialize(rawConcert, CamelCaseOptions);
        using var doc = JsonDocument.Parse(json);
        var statusElement = doc.RootElement.GetProperty("status");

        Assert.Equal(JsonValueKind.String, statusElement.ValueKind);
        Assert.Equal(expectedStatusString, statusElement.GetString());
    }

    [Theory]
    [InlineData("Planned", ConcertDto.ConcertStatusValue.Planned)]
    [InlineData("Running", ConcertDto.ConcertStatusValue.Running)]
    [InlineData("Past", ConcertDto.ConcertStatusValue.Past)]
    [InlineData("Cancelled", ConcertDto.ConcertStatusValue.Cancelled)]
    public void RawConcertDto_DeserializesStatusFromString(string statusString, ConcertDto.ConcertStatusValue expectedStatus)
    {
        var json = $"{{\"id\":\"c123\",\"status\":\"{statusString}\"}}";
        var deserialized = JsonSerializer.Deserialize<RawConcertDto>(json, CamelCaseOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(expectedStatus, deserialized.Status);
    }

    [Theory]
    [InlineData(ConcertDto.ConcertStatusValue.Planned, "Planned")]
    [InlineData(ConcertDto.ConcertStatusValue.Running, "Running")]
    [InlineData(ConcertDto.ConcertStatusValue.Past, "Past")]
    [InlineData(ConcertDto.ConcertStatusValue.Cancelled, "Cancelled")]
    public void CreateConcertRequestDto_SerializesStatusAsString(ConcertDto.ConcertStatusValue status, string expectedStatusString)
    {
        var request = new CreateConcertRequestDto
        {
            ConcertTypeId = 1,
            VenueId = 2,
            PostedStartTime = DateTimeOffset.UtcNow,
            Status = status
        };

        var json = JsonSerializer.Serialize(request, CamelCaseOptions);
        using var doc = JsonDocument.Parse(json);
        var statusElement = doc.RootElement.GetProperty("status");

        Assert.Equal(JsonValueKind.String, statusElement.ValueKind);
        Assert.Equal(expectedStatusString, statusElement.GetString());
    }

    [Theory]
    [InlineData("Planned", ConcertDto.ConcertStatusValue.Planned)]
    [InlineData("Running", ConcertDto.ConcertStatusValue.Running)]
    [InlineData("Past", ConcertDto.ConcertStatusValue.Past)]
    [InlineData("Cancelled", ConcertDto.ConcertStatusValue.Cancelled)]
    public void CreateConcertRequestDto_DeserializesStatusFromString(string statusString, ConcertDto.ConcertStatusValue expectedStatus)
    {
        var json = $"{{\"concertTypeId\":1,\"venueId\":2,\"postedStartTime\":\"2026-09-01T20:00:00Z\",\"status\":\"{statusString}\"}}";
        var deserialized = JsonSerializer.Deserialize<CreateConcertRequestDto>(json, CamelCaseOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(expectedStatus, deserialized.Status);
    }
}
