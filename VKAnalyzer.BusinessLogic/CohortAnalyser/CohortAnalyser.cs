using System.Collections.Generic;
using System.Linq;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser
{
    public class CohortAnalyser
    {
        public string[,] BuildCohortAnalyseData(IEnumerable<CohortAnalysisModel> data)
        {
            var invertedData = data.OrderBy(d => d.PostDate).ToList();
            var invertedDataCpunt = invertedData.Count();
            var result = new string[invertedDataCpunt, invertedDataCpunt];

            for (var h = 0; h < invertedDataCpunt; h++)
            {
                for (var g = 0; g < invertedDataCpunt; g++)
                {
                    if (h == 0 && g == 0)
                    {
                        //Первая запись в матрице
                        result[0, 0] = invertedData[0].LikedIds.Count().ToString();
                    }
                    if (h == g && g > 0)
                    {
                        var currentCohort = invertedData[g].LikedIds.ToList();

                        var allPreviuosUsers = new List<string>();

                        for (var i = g; i >= g; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни для последующего вычисления новых уникальных
                            allPreviuosUsers = allPreviuosUsers.Concat(invertedData[i - 1].LikedIds).ToList();
                        }

                        //Новые уникальные пользователи
                        var newUsersCount = currentCohort.Count(q => !allPreviuosUsers.Contains(q));
                        result[g, g] = newUsersCount.ToString(); // запись в таблицу самых новых пользователей

                        //Пользователи за предыдущие дни
                        for (var i = g; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            var previusCohort = invertedData[i - 1].LikedIds;
                            var usersFromPreviousCohortCount = currentCohort.Count(previusCohort.Contains);
                            result[i - 1, g] = usersFromPreviousCohortCount.ToString();
                        }
                    }
                }
            }

            return result;
        }
    }
}
