using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Responses;

namespace Lambda.Auth;

[JsonSerializable(typeof(ErrorResponse))]
internal partial class AuthJsonContext : JsonSerializerContext
{
}