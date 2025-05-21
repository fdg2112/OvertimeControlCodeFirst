namespace OvertimeControlCodeFirst.Helpers
{
    public class DateHelper
    {
        public static int CurrentMonth => DateTime.Now.Month;

        public static int CurrentYear => DateTime.Now.Year;

        public static DateTime StartOfCurrentMonth =>
            new DateTime(CurrentYear, CurrentMonth, 1);

        public static DateTime StartOfPreviousMonth =>
            StartOfCurrentMonth.AddMonths(-1);

        public static DateTime StartOf12MonthsAgo =>
            StartOfCurrentMonth.AddMonths(-11);

        public static bool IsSameMonthAndYear(DateTime date) =>
            date.Month == CurrentMonth && date.Year == CurrentYear;

    }
}
