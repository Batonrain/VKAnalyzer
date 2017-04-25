using System.Collections.Generic;
using System.Linq;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser
{
    public class CohortAnalyser
    {
        public List<string>[,] BuildCohortAnalyseData(List<CohortAnalysisModel> data)
        {
            var dataCount = data.Count();
            var result = new List<string>[dataCount, dataCount];

            for (var h = 0; h < dataCount; h++)
            {
                for (var v = 0; v < dataCount; v++)
                {
                    if (h == 0 && v == 0)
                    {
                        //Первая запись в матрице
                        result[0, 0] = data[0].LikedIds.ToList();
                    }
                    if (h == v && v > 0)
                    {
                        var currentCohort = data[v].LikedIds.ToList();

                        var allPreviuosUsers = new List<string>();

                        for (var i = v; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни для последующего вычисления новых уникальных
                            allPreviuosUsers.AddRange(result[i - 1, i - 1]);
                        }

                        //Новые уникальные пользователи
                        var newUsersCount = currentCohort.Where(q => !allPreviuosUsers.Contains(q));
                        result[v, v] = newUsersCount.ToList(); // запись в таблицу самых новых пользователей

                        //Пользователи за предыдущие дни
                        for (var i = v; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            //var previusCohort = invertedData[i - 1].LikedIds;
                            var previusCohortCell = result[i - 1, i - 1];
                            //result[i - 1, v] = currentCohort.Where(previusCohortCell.Contains).ToList();
                            result[i - 1, v] = previusCohortCell.Intersect(currentCohort).ToList();
                        }
                    }
                }
            }

            return result;
        }
    }
}
