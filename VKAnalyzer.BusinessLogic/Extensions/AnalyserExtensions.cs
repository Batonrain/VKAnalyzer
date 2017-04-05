using System.Collections.Generic;
using System.Linq;

namespace VKAnalyzer.BusinessLogic.Extensions
{
    public class AnalyserExtensions
    {
        public List<T> IntersectAll<T>(IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T> hashSet = null;
            foreach (var list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = new HashSet<T>(list);
                }
                else
                {
                    hashSet.IntersectWith(list);
                }
            }
            return hashSet == null ? new List<T>() : hashSet.ToList();
        }
    }
}
