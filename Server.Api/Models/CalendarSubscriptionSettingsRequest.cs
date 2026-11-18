using System.ComponentModel.DataAnnotations;

namespace Server.Api.Models;

/// <summary>Settings required when creating or replacing a calendar subscription.</summary>
public sealed class CalendarSubscriptionSettingsRequest
{
    /// <summary>Whether to include the doors opening time.</summary>
    [Required]
    public bool? IncludeDoorsTime { get; set; }

    /// <summary>Whether to include the Linkin Park stage time.</summary>
    [Required]
    public bool? IncludeLpStageTime { get; set; }
}
