using System.ComponentModel;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser
{
    public enum AnalyseStep
    {
        [Description("День")]
        Day,

        [Description("Неделя")]
        Week,

        [Description("Две недели")]
        TwoWeeks,

        [Description("Месяц")]
        Month
    }
}
