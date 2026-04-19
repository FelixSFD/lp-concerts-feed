using System.Text.Json;
using Xunit;

namespace Lambda.Push.Sender.Test;


public class NotificationJsonSerializerTest
{
    [Fact]
    public void SilentNotification()
    {
        var payload = new NotificationWrapper<AppleNotificationBackground>
        {
            Apple = new AppleNotificationBackground
            {
                ContentAvailable = true,
            },
            TriggerSync = true,
        };

        var json = JsonSerializer.Serialize(payload, NotificationJsonSerializer.Default.NotificationWrapperAppleNotificationBackground);
        const string expectedJson = "{\"aps\":{\"content-available\":1},\"triggerSync\":true}";
        Assert.Equal(expectedJson, json);
    }
    
    
    [Theory]
    [InlineData("Stage Time Confirmed", "Some subtitle", "Some Body", "1234567", true, "confirmedStageTime", "1234")]
    public void StageTimeConfirmed(string title, string subtitle, string body, string threadId, bool isMutable, string category, string concertId)
    {
        var payload = new NotificationWrapper<AppleNotificationAlert>
        {
            Apple = new AppleNotificationAlert
            {
                Alert = new AppleNotificationPayload
                {
                    Title = title,
                    Subtitle = subtitle,
                    Body = body
                },
                ThreadId = threadId,
                MutableContent =  isMutable,
                Category = category,
            },
            ConcertId = concertId
        };

        var json = JsonSerializer.Serialize(payload, NotificationJsonSerializer.Default.NotificationWrapperAppleNotificationAlert);
        var expectedJson = "{\"aps\":{\"alert\":{\"title\":\"" + title + "\",\"subtitle\":\"" + subtitle + "\",\"body\":\"" + body + "\"},\"thread-id\":\"" + threadId + "\",\"category\":\"" + category + "\",\"mutable-content\":" + (isMutable ? 1 : 0) + "},\"concertId\":\"" + concertId + "\"}";
        Assert.Equal(expectedJson, json);
    }
}