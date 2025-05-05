using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Enums
{
    public enum HourType
    {
        [Display(Name = "50%")]
        FiftyPercent = 50,
        [Display(Name = "100%")]
        OneHundredPercent = 100
    }
}
