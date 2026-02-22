using System.Text.Json;
using Xunit;

namespace Lambda.Push.Sender.Test;


public class NotificationJsonSerializerTest
{
    [Fact]
    public void SilentNotification()
    {
        var payload = new NotificationWrapper
        {
            Apple = new AppleNotificationBackground
            {
                ContentAvailable = true,
            },
            TriggerSync = true,
        };

        var json = JsonSerializer.Serialize(payload, NotificationJsonSerializer.Default.NotificationWrapper);
        const string expectedJson = "{\"aps\":{},\"triggerSync\":true}";
        Assert.Equal(expectedJson, json);
    }
    
    
    [Theory]
    [InlineData("Stage Time Confirmed", "Some Body", "1234567", true, "confirmedStageTime", "1234")]
    public void StageTimeConfirmed(string title, string body, string threadId, bool isMutable, string category, string concertId)
    {
        var payload = new NotificationWrapper
        {
            Apple = new AppleNotificationAlert
            {
                Alert = new AppleNotificationPayload
                {
                    Title = title,
                    Body = body
                },
                ThreadId = threadId,
                MutableContent =  isMutable,
                Category = category,
            },
            ConcertId = concertId
        };

        var json = JsonSerializer.Serialize(payload, NotificationJsonSerializer.Default.NotificationWrapper);
        var expectedJson = "{\"aps\":{\"alert\":{\"title\":\"" + title + "\",\"body\":\"" + body + "\",\"category\":\"" + category + "\",\"mutable-content\":" + (isMutable ? 1 : 0) + "},\"concertId\":\"" + concertId + "\"}";
        Assert.Equal(expectedJson, json);
    }
}