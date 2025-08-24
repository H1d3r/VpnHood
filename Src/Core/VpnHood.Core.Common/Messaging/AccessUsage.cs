﻿using System.Text.Json.Serialization;

namespace VpnHood.Core.Common.Messaging;

public class AccessUsage
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CanExtendByRewardedAd { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Obsolete]
    public bool IsUserReviewRecommended { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int UserReviewRecommended { get; set; }

    public bool IsPremium { get; set; } // session premium
    public Traffic CycleTraffic { get; set; } = new(); // total traffic
    public Traffic TotalTraffic { get; set; } = new(); // total traffic
    public long MaxTraffic { get; set; } // session max traffic
    public DateTime? ExpirationTime { get; set; } // It is the session expiration not access expiration
    public int? ActiveClientCount { get; set; }
}