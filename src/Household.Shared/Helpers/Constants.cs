namespace Household.Shared.Helpers;

public class EpisodeText
{
    public const string ComingSoonLabel = "Coming Soon";
    public const string SeasonFinaleLabel = "Season Finale";
    public const string LastEpisodeLabel = "Last Episode";
}

public class EpisodeBadge
{
    public const string BackgroundLight = "bg-light";
    public const string BackgroundInfo = "bg-info";
    public const string BackgroundWarning = "bg-warning";
    public const string BackgroundDanger = "bg-danger";
    public const string BackgroundSuccess = "bg-success";

    public const string TextColorLight = "text-light";
    public const string TextColorDark = "text-dark";
}

public class CacheKeys
{
    // Key: <Instance>_<Key>:<Name>
    // Key: HouseholdCache_Household:DailyShows
    // Not having instance name works because it is setup in program.cs
    public const string DailyShowsKey = "Household:DailyShows";
    public const string MoviesKey = "Household:Movies";
    public const string DvdTypesKey = "Household:DvdTypes";
    public const string ShowInformationSelectorKey = "Household:ShowInformationSelector";
    public const string StreamingServicesKey = "Household:StreamingServices";
    public const string SubscriptionSelectorKey = "Household:SubscriptionSelector";
    public const string AccessToken = "Keycloak:AccessToken";
    public const string ExpiresAt = "Keycloak:ExpiresAt";
    //public const string UserSubscriptionsKey = "Household:UserSubscriptions";
}