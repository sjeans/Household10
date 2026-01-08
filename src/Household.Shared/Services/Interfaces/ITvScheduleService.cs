using Household.Shared.Dtos;

namespace Household.Shared.Services.Interfaces;

public interface ITvScheduleService
{
    string GetSeriesStatus(TVShowInformationDto showInfo, DateTime? todayOverride, int whichMessage);
}
