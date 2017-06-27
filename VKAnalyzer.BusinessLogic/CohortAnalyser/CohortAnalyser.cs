using System;
using System.Collections.Generic;
using System.Linq;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser
{
    public class CohortAnalyser
    {
        public CohortAnalysisResultModel Analyze(List<CohortAnalysisModel> models, int step, DateTime dateOfTheBeginning, DateTime dateOfTheEnd, string groupId)
        {
            var result = new CohortAnalysisResultModel();

            var preparedData = PrepareDataForCohortAnalyse(models, step, dateOfTheBeginning, dateOfTheEnd).OrderBy(d => d.PostDate).ToList();

            result = Analyse(preparedData, groupId);

            if (step == 1)
            {
                for (var dt = dateOfTheBeginning; dt < dateOfTheEnd; dt = dt.AddDays(1))
                {
                    result.Dates.Add(dt.ToShortDateString());
                }
            }

            if (step == 2)
            {
                var allDays = GetTotalDays(dateOfTheBeginning, dateOfTheEnd);
                var st = 7;
                var countOfSteps = Math.Ceiling(allDays / st);
                for (var i = 0; i < countOfSteps; i++)
                {
                    var startDate = dateOfTheBeginning.AddDays(i * st);
                    var endDate = dateOfTheBeginning.AddDays((i + 1) * st);
                    if (endDate > dateOfTheEnd)
                    {
                        endDate = dateOfTheEnd;
                    }

                    result.Dates.Add(string.Format("{0} - {1}", startDate.ToShortDateString(), endDate.ToShortDateString()));
                }
            }

            if (step == 3)
            {
                var allDays = GetTotalDays(dateOfTheBeginning, dateOfTheEnd);
                var st = 30;
                var countOfSteps = Math.Ceiling(allDays / st);
                for (var i = 0; i < countOfSteps; i++)
                {
                    var startDate = dateOfTheBeginning.AddDays(i * st);
                    var endDate = dateOfTheBeginning.AddDays((i + 1) * st);
                    if (endDate > dateOfTheEnd)
                    {
                        endDate = dateOfTheEnd;
                    }

                    result.Dates.Add(string.Format("{0} - {1}", startDate.ToShortDateString(), endDate.ToShortDateString()));
                }
            }

            return result;
        }

        private CohortAnalysisResultModel Analyse(List<CohortAnalysisModel> preparedData, string groupId)
        {
            var analyser = new CohortAnalyser();

            var absoluteData = analyser.BuildCohortAnalyseData(preparedData);
            var relativeData = analyser.BuildRelativeValues(absoluteData);
            var relativeDataWithShift = analyser.BuildRelativeValuesWithShift(absoluteData);

            var result = new CohortAnalysisResultModel
            {
                AbsoluteValues = absoluteData,
                RelativeValues = relativeData,
                RelativeValuesWithShift = relativeDataWithShift,
                TableLength = preparedData.Count,
                GroupId = groupId
            };

            result.TotalHorizontal = CountTotalHorizontal(result.AbsoluteValues);
            result.TotalVertical = CountTotalVertical(result.AbsoluteValues);

            return result;
        }

        //Главный когоротный анализатор. Построение матрицы, высчитывание результатов.
        private List<string>[,] BuildCohortAnalyseData(List<CohortAnalysisModel> data)
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
                            var previusCohortCell = result[i - 1, i - 1];
                            
                            result[i - 1, v] = previusCohortCell.Intersect(currentCohort).ToList();
                        }
                    }
                }
            }

            return result;
        }

        private string[,] BuildRelativeValues(List<string>[,] data)
        {
            var arrayLength = data.GetLength(1);
            var result = new string[arrayLength, arrayLength];

            for (var h = 0; h < arrayLength; h++)
            {
                for (var v = 0; v < arrayLength; v++)
                {
                    if (h == v)
                    {
                        result[h, v] = "100%";

                        for (var i = v + 1; i < arrayLength; i++)
                        {
                            var percentResult = Math.Truncate((double)data[h, i].Count() / data[h, v].Count() * 100);

                            result[h, i] = string.Format("{0}%", percentResult);
                        }
                    }
                }
            }

            return result;
        }

        private string[,] BuildRelativeValuesWithShift(List<string>[,] data)
        {
            var arrayLength = data.GetLength(1);
            var result = new string[arrayLength, arrayLength];

            for (var h = 0; h < arrayLength; h++)
            {
                for (var v = 0; v < arrayLength; v++)
                {
                    if (h == v)
                    {
                        result[h, 0] = "100%";

                        var vert = 1;
                        for (var i = v + 1; i < arrayLength; i++)
                        {
                            var percentResult = Math.Truncate((double)data[h, i].Count() / data[h, v].Count() * 100);

                            result[h, vert] = string.Format("{0}%", percentResult);
                            vert++;
                        }
                    }
                }
            }

            return result;
        }

        private IEnumerable<CohortAnalysisModel> PrepareDataForCohortAnalyse(List<CohortAnalysisModel> posts, int step, DateTime dateOfTheBeginning, DateTime dateOfTheEnd)
        {
            var result = new List<CohortAnalysisModel>();

            if (step == 1)
            {
                var getAllDays = posts.Select(p => p.PostDate.Date).Distinct();

                result.AddRange(getAllDays.Select(day => new CohortAnalysisModel()
                {
                    PostDate = day,
                    LikedIds = posts.Where(p => p.PostDate.Date == day).SelectMany(s => s.LikedIds).Distinct().ToList()
                }));
            }
            if (step == 2)
            {
                var allDays = GetTotalDays(dateOfTheBeginning, dateOfTheEnd);
                var st = 7;
                var countOfSteps = Math.Ceiling(allDays / st);

                return GetOrderedDataForAnalyse(st, countOfSteps, dateOfTheBeginning, posts);
            }

            if (step == 3)
            {
                var allDays = GetTotalDays(dateOfTheBeginning, dateOfTheEnd);
                var st = 30;
                var countOfSteps = Math.Ceiling(allDays / st);

                return GetOrderedDataForAnalyse(st, countOfSteps, dateOfTheBeginning, posts);
            }

            return result;
        }

        private IEnumerable<CohortAnalysisModel> GetOrderedDataForAnalyse(int step, double countOfSteps, DateTime dateOfTheBeginning, List<CohortAnalysisModel> posts)
        {
            var result = new List<CohortAnalysisModel>();

            for (var i = 0; i < countOfSteps; i++)
            {
                var startDate = dateOfTheBeginning.AddDays(i * step);
                var endDate = dateOfTheBeginning.AddDays((i + 1) * step);

                var res = new CohortAnalysisModel
                {
                    PostDate = dateOfTheBeginning.AddDays(i),
                    LikedIds = posts.Where(p => startDate <= p.PostDate.Date && p.PostDate.Date < endDate).SelectMany(s => s.LikedIds).Distinct().ToList()
                };

                result.Add(res);
            }

            return result;
        }

        private List<int> CountTotalHorizontal(List<string>[,] resultMatrix)
        {
            var result = new List<int>();

            for (var i = 0; i < resultMatrix.GetLength(0); i++) // for 1
            {
                var summ = 0;

                for (var j = 0; j < resultMatrix.GetLength(0); j++) // for 2
                {
                    if (resultMatrix[i, j] != null && resultMatrix[i, j].Count > 0)
                        summ += resultMatrix[i, j].Count;
                } // for 2

                result.Add(summ);

            } // for 1

            return result;
        }

        private List<int> CountTotalVertical(List<string>[,] resultMatrix)
        {
            var result = new List<int>();

            // Обсчитываем массив
            for (var i = 0; i < resultMatrix.GetLength(0); i++) // for 1
            {
                var summ = 0;

                for (var j = 0; j < resultMatrix.GetLength(0); j++) // for 2
                {
                    if (resultMatrix[j, i] != null && resultMatrix[j, i].Count > 0)
                        summ += resultMatrix[j, i].Count;
                } // for 2

                result.Add(summ);

            } // for 1

            return result;
        }

        private double GetTotalDays(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).TotalDays;
        }
    }
}
