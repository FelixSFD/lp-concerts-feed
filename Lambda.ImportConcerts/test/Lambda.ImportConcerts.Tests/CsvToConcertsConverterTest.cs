using JetBrains.Annotations;
using LPCalendar.DataStructure;
using Xunit;

namespace Lambda.ImportConcerts.Tests;

[TestSubject(typeof(CsvToConcertsConverter))]
public class CsvToConcertsConverterTest
{

    [Fact]
    public void Converter()
    {
        var input = File.ReadAllText("TestData/test_import.csv");

        Concert[] expectedConcerts = [
            new()
            {
                Status = "PUBLISHED",
                Id = "a1d1e58f-1843-416d-ae08-9404308d67a2",
                PostedStartTime = "2025-07-01T18:00:00.0000000+02:00",
                TourName = "FROM ZERO WORLD TOUR 2025",
                Venue = "Merkur Spiel Arena",
                City = "Düsseldorf",
                State = "NRW",
                Country = "Germany",
                TimeZoneId = "Europe/Berlin"
            },
            new()
            {
                Status = "PUBLISHED",
                Id = "57d21142-518c-40e0-8eb1-0472f6ced3c4",
                PostedStartTime = "2025-02-03T21:00:00.0000000-06:00",
                TourName = "FROM ZERO WORLD TOUR 2025",
                Venue = "Estadio 3 de Marzo",
                City = "Guadalajara",
                State = null,
                Country = "Mexico",
                TimeZoneId = "America/Mexico_City"
            }
        ];

        var parsed = CsvToConcertsConverter
            .GetConcerts(input)
            .ToArray();
        
        Assert.Equal(expectedConcerts, parsed, (a, b) => a.Id == b.Id
                                                         && a.Venue == b.Venue
                                                         && a.City == b.City
                                                         && a.State == b.State
                                                         && a.Country == b.Country
                                                         && a.TourName == b.TourName
                                                         && a.Status == b.Status
                                                         && a.PostedStartTime == b.PostedStartTime);
    }
}