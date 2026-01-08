using System.ComponentModel;

namespace Household.Shared.Enums;

public enum DvdTypes
{
    [Description("HD DVD")]
    HdDvd = 1,
    [Description("DVD")]
    Dvd = 2,
    [Description("Blue Ray")]
    BlueRay = 3,
}
