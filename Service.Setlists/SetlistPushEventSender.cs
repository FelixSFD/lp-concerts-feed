using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Events.PushNotifications;

namespace Service.Setlists;

/// <summary>
/// Class that sends notifications to the queue when something happened with a setlist. (like a new live premiere)
/// </summary>
public class SetlistPushEventSender(IAmazonSQS sqsClient, ILambdaLogger logger) : ISetlistPushEventSender
{
    /// <inheritdoc/>
    public async Task SendLivePremiere(string setlistEntryTitle, ConcertDto concert)
    {
        var pushEvent = new SetlistSongPremiereNotificationEvent
        {
            SetlistEntryTitle = setlistEntryTitle,
            Concert = concert,
        };
        
        logger.LogDebug("Will send notification of type '{type}' for concert with id '{id}' and entry: {entryTitle}", PushNotificationType.SetlistSongPremiereAlert, pushEvent.Concert.Id, setlistEntryTitle);
        
        var sqsMessage = new SendMessageRequest
        {
            MessageGroupId = "default",
            QueueUrl = Environment.GetEnvironmentVariable("PUSH_QUEUE_URL"),
            MessageBody = JsonSerializer.Serialize(pushEvent, DataStructureJsonContext.Default.SetlistSongPremiereNotificationEvent),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "notificationType", new MessageAttributeValue
                    {
                        StringValue = nameof(PushNotificationType.SetlistSongPremiereAlert),
                        DataType = "String"
                    }
                }
            }
        };
        
        logger.LogDebug("Prepared SQS Message");
        //context.Logger.LogDebug("Sending SQS message: {json}", JsonSerializer.Serialize(sqsMessage, LocalJsonSerializer.Default.SendMessageRequest));

        await sqsClient.SendMessageAsync(sqsMessage);
        //await StoreConcertNotificationHistory(concert, context);
    }
}