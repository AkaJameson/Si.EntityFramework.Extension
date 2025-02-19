using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Si.EntityFramework.Extension.DataBase.Extensions
{
    public static class JsonColumnExtensions
    {
        /// <summary>
        /// 将属性配置为JSON存储
        /// </summary>
        public static PropertyBuilder<T> HasJsonConversion<T>(
            this PropertyBuilder<T> propertyBuilder,
            JsonSerializerOptions options = null) where T : class
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var converter = new ValueConverter<T, string>(
                // 将对象转换为JSON字符串
                v => JsonSerializer.Serialize(v, options),
                // 将JSON字符串转换回对象
                v => JsonSerializer.Deserialize<T>(v, options)
            );

            var comparer = new ValueComparer<T>(
                // 比较两个对象是否相等
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                // 生成对象的哈希码
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                // 创建对象的快照
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }

        /// <summary>
        /// 将集合属性配置为JSON存储
        /// </summary>
        public static PropertyBuilder<List<T>> HasJsonConversion<T>(
            this PropertyBuilder<List<T>> propertyBuilder,
            JsonSerializerOptions options = null)
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var converter = new ValueConverter<List<T>, string>(
                // 将列表转换为JSON字符串
                v => JsonSerializer.Serialize(v, options),
                // 将JSON字符串转换回列表
                v => JsonSerializer.Deserialize<List<T>>(v, options)
            );

            var comparer = new ValueComparer<List<T>>(
                // 比较两个列表是否相等
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                // 生成列表的哈希码
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                // 创建列表的快照
                v => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(v, options), options)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}
