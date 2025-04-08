using dotnet_api_extensions.Extensions;

using System.Linq.Expressions;

namespace dotnet_api_extensions.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// x=> x.PropertyName.ToString()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static IDictionary<string, IDictionary<string, List<T>>> GroupBy<T>(this IList<T> sourceList, IList<string> propertyNames)
        {
            if (sourceList == null) return default;

            var dictionaries = new Dictionary<string, IDictionary<string, List<T>>>();

            foreach (string propertyName in propertyNames)
            {
                var keySelector = propertyName.StringKeySelector<T>();
                dictionaries[propertyName] = sourceList.GroupBy(keySelector).ToDictionary(x => x.Key, x => x.ToList());
            }

            return dictionaries;
        }

        /// <summary>
        /// Generates key selector using equalityComparerPropertyName and ToString method
        /// Result is : x=> x.equalityComparerPropertyName.ToString();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalityComparerPropertyName"></param>
        /// <returns></returns>
        public static Func<T, string> StringKeySelector<T>(this string equalityComparerPropertyName)
        {
            var param = Expression.Parameter(typeof(T));
            var body = Expression.Call(
                    Expression.Property(param, equalityComparerPropertyName),
                    "ToString",
                    null
                );

            return Expression.Lambda<Func<T, string>>(body, param).Compile();
        }

        /// <summary>
        /// Checks if keyselector is null. If yes, then create a default one. Default selector : x=> x.ID
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static Func<TItem, TKey> OrDefault<TItem, TKey>(this Func<TItem, TKey> keySelector)
        {
            if (keySelector != null) return keySelector;

            var parameter = Expression.Parameter(typeof(TItem), "x");
            var member = Expression.Property(parameter, "ID"); //x.ID
            return Expression.Lambda<Func<TItem, TKey>>(member, parameter).Compile();
        }
    }
}