namespace Common.Database.Tests;

public class SortDescriptorTest
{
    [Theory]
    [InlineData("Date", "Date", false)]
    [InlineData("-Date", "Date", true)]
    [InlineData("-PostedStartTime", "PostedStartTime", true)]
    [InlineData("Venue.CurrentName", "Venue.CurrentName", false)]
    [InlineData("-Venue.CurrentName", "Venue.CurrentName", true)]
    [InlineData("Something-With-Dash", "Something-With-Dash", false)]
    [InlineData("-Something-With-Dash", "Something-With-Dash", true)]
    public void FromString(string input, string expectedProperty, bool expectedDescending)
    {
        var descriptor = SortDescriptor.FromString(input);
        Assert.Equal(expectedProperty, descriptor.Property);
        Assert.Equal(expectedDescending, descriptor.Descending);
    }
}