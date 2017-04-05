using System.Collections.Generic;
using System.Linq;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser
{
    public class CohortAnalyser
    {
        public List<string>[,] BuildCohortAnalyseData(IEnumerable<CohortAnalysisModel> data)
        {
            var invertedData = data.OrderBy(d => d.PostDate).ToList();
            var invertedDataCount = invertedData.Count();
            var result = new List<string>[invertedDataCount, invertedDataCount];

            for (var h = 0; h < invertedDataCount; h++)
            {
                for (var v = 0; v < invertedDataCount; v++)
                {
                    if (h == 0 && v == 0)
                    {
                        //Первая запись в матрице
                        result[0, 0] = invertedData[0].LikedIds.ToList();
                    }
                    if (h == v && v > 0)
                    {
                        var currentCohort = invertedData[v].LikedIds.ToList();

                        var allPreviuosUsers = new List<string>();

                        for (var i = v; i >= v; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни для последующего вычисления новых уникальных
                            allPreviuosUsers = allPreviuosUsers.Concat(invertedData[i - 1].LikedIds).ToList();
                        }

                        //Новые уникальные пользователи
                        var newUsersCount = currentCohort.Where(q => !allPreviuosUsers.Contains(q));
                        result[v, v] = newUsersCount.ToList(); // запись в таблицу самых новых пользователей

                        //Пользователи за предыдущие дни
                        for (var i = h; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            //var previusCohort = invertedData[i - 1].LikedIds;
                            var previusCohortCell = result[i - 1, v - 1];
                            result[i - 1, v] = currentCohort.Where(previusCohortCell.Contains).ToList();
                        }
                    }
                }
            }

            return result;
        }

        public string[,] BuildCohortAnalyseData1(IEnumerable<CohortAnalysisModel> data)
        {
            var re = BuildCohortAnalyseData1(data);

            var invertedData = data.OrderBy(d => d.PostDate).ToList();
            var invertedDataCpunt = invertedData.Count();
            var result = new string[invertedDataCpunt, invertedDataCpunt];

            for (var h = 0; h < invertedDataCpunt; h++)
            {
                for (var v = 0; v < invertedDataCpunt; v++)
                {
                    if (h == 0 && v == 0)
                    {
                        //Первая запись в матрице
                        result[0, 0] = invertedData[0].LikedIds.Count().ToString();
                    }
                    if (h == v && v > 0)
                    {
                        var currentCohort = invertedData[v].LikedIds.ToList();

                        var allPreviuosUsers = new List<string>();

                        for (var i = v; i >= v; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни для последующего вычисления новых уникальных
                            allPreviuosUsers = allPreviuosUsers.Concat(invertedData[i - 1].LikedIds).ToList();
                        }

                        //Новые уникальные пользователи
                        var newUsersCount = currentCohort.Count(q => !allPreviuosUsers.Contains(q));
                        result[v, v] = newUsersCount.ToString(); // запись в таблицу самых новых пользователей

                        //

                        //Пользователи за предыдущие дни
                        for (var i = v; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            var previusCohort = invertedData[i - 1].LikedIds;
                            var usersFromPreviousCohortCount = currentCohort.Count(previusCohort.Contains);
                            result[i - 1, v] = usersFromPreviousCohortCount.ToString();
                        }
                    }
                }
            }

            return result;
        }
    }
}
