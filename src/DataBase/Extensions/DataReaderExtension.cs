using System.Data;

namespace Si.EntityFramework.Extension.DataBase.Extensions
{
    public static class DataReaderExtension
    {
        public static ICollection<T> ToObjectCollection<T>(this IDataReader dataReader) where T : new()
        {
            var results = new List<T>();
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanWrite)
                .ToList();

            while (dataReader.Read())
            {
                var obj = new T();

                foreach (var property in properties)
                {
                    var columnName = property.Name;
                    if (ColumnExists(dataReader, columnName))
                    {
                        var value = dataReader[columnName];
                        if (value != DBNull.Value)
                        {
                            property.SetValue(obj, value);
                        }
                    }
                }
                results.Add(obj);
            }

            return results;
        }

        private static bool ColumnExists(IDataReader dataReader, string columnName)
        {
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (dataReader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
