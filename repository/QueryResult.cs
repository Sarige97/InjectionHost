using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using moju.log;

namespace moju.repository
{
    /// <summary>
    /// 列名映射特性，用于指定字典中的键名与实体字段/属性的映射关系
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ColumnNameAttribute : Attribute
    {
        /// <summary>
        /// 映射的列名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否忽略该字段/属性（不进行映射）
        /// </summary>
        public bool Ignore { get; set; }

        public ColumnNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 忽略该字段/属性
        /// </summary>
        public ColumnNameAttribute()
        {
            Ignore = true;
        }
    }

    /// <summary>
    /// 查询结果类，用于将数据库查询结果转换为实体对象列表
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class QueryResult<T> where T : new()
    {
        /// <summary>
        /// 转换后的实体列表
        /// </summary>
        public List<T> ResultList { get; private set; }

        /// <summary>
        /// 原始查询结果（字典列表）
        /// </summary>
        public List<Dictionary<string, object>> OriginResultDictionary { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="originResultDictionary">原始查询结果</param>
        public QueryResult(List<Dictionary<string, object>> originResultDictionary)
        {
            this.OriginResultDictionary = originResultDictionary ?? new List<Dictionary<string, object>>();
            this.ResultList = new List<T>();

            // 执行数据转换
            ConvertData();
        }

        /// <summary>
        /// 转换数据：将字典列表转换为实体列表
        /// </summary>
        private void ConvertData()
        {
            if (OriginResultDictionary == null || OriginResultDictionary.Count == 0)
                return;

            Type type = typeof(T);

            // 获取所有可映射的字段和属性（缓存起来提高性能）
            var mappingInfo = GetMappingInfo(type);

            foreach (var dict in OriginResultDictionary)
            {
                T instance = new T();

                // 处理字段
                foreach (var fieldInfo in mappingInfo.Fields)
                {
                    string key = fieldInfo.Key;
                    if (dict.TryGetValue(key, out object value))
                    {
                        try
                        {
                            object convertedValue = ConvertValue(value, fieldInfo.FieldType);
                            fieldInfo.FieldInfo.SetValue(instance, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            // 记录转换失败日志
                            SimpleLogger.Instance.Error($"字段 {fieldInfo.FieldInfo.Name} 转换失败: {ex.Message}");
                        }
                    }
                }

                // 处理属性
                foreach (var propInfo in mappingInfo.Properties)
                {
                    string key = propInfo.Key;
                    if (dict.TryGetValue(key, out object value))
                    {
                        try
                        {
                            object convertedValue = ConvertValue(value, propInfo.PropertyType);
                            propInfo.PropertyInfo.SetValue(instance, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            // 记录转换失败日志
                            SimpleLogger.Instance.Error($"属性 {propInfo.PropertyInfo.Name} 转换失败: {ex.Message}");
                        }
                    }
                }

                ResultList.Add(instance);
            }
        }

        /// <summary>
        /// 获取类型的映射信息（字段和属性）
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>映射信息</returns>
        private MappingInfo GetMappingInfo(Type type)
        {
            var mappingInfo = new MappingInfo();

            // 获取所有公共实例字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // 检查是否有 ColumnName 特性
                var attr = field.GetCustomAttribute<ColumnNameAttribute>();

                // 如果标记为 Ignore，跳过
                if (attr != null && attr.Ignore)
                    continue;

                // 获取列名：优先使用特性指定的名称，否则使用字段名
                string columnName = attr != null && !string.IsNullOrEmpty(attr.Name)
                    ? attr.Name
                    : field.Name;

                mappingInfo.Fields.Add(new FieldMappingInfo
                {
                    FieldInfo = field,
                    Key = columnName,
                    FieldType = field.FieldType
                });
            }

            // 获取所有公共实例属性（可写的）
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            foreach (var property in properties)
            {
                // 检查是否有 ColumnName 特性
                var attr = property.GetCustomAttribute<ColumnNameAttribute>();

                // 如果标记为 Ignore，跳过
                if (attr != null && attr.Ignore)
                    continue;

                // 获取列名：优先使用特性指定的名称，否则使用属性名
                string columnName = attr != null && !string.IsNullOrEmpty(attr.Name)
                    ? attr.Name
                    : property.Name;

                mappingInfo.Properties.Add(new PropertyMappingInfo
                {
                    PropertyInfo = property,
                    Key = columnName,
                    PropertyType = property.PropertyType
                });
            }

            return mappingInfo;
        }

        /// <summary>
        /// 转换值的类型
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private object ConvertValue(object value, Type targetType)
        {
            // 处理 null 和 DBNull
            if (value == null || value == DBNull.Value)
            {
                return GetDefaultValue(targetType);
            }

            // 处理可空类型
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    try
                    {
                        return Convert.ChangeType(value, underlyingType);
                    }
                    catch
                    {
                        return GetDefaultValue(targetType);
                    }
                }
            }

            // 处理枚举
            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.ToObject(targetType, value);
                }
                catch
                {
                    return Enum.Parse(targetType, value.ToString(), true);
                }
            }

            // 处理 Guid
            if (targetType == typeof(Guid))
            {
                if (value is Guid)
                    return value;

                string str = value.ToString();
                if (string.IsNullOrEmpty(str))
                    return Guid.Empty;

                return Guid.Parse(str);
            }

            // 处理字符串到其他类型的转换
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 如果转换失败，返回默认值
                return GetDefaultValue(targetType);
            }
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>默认值</returns>
        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        /// <summary>
        /// 映射信息
        /// </summary>
        private class MappingInfo
        {
            public List<FieldMappingInfo> Fields { get; set; } = new List<FieldMappingInfo>();
            public List<PropertyMappingInfo> Properties { get; set; } = new List<PropertyMappingInfo>();
        }

        /// <summary>
        /// 字段映射信息
        /// </summary>
        private class FieldMappingInfo
        {
            public FieldInfo FieldInfo { get; set; }
            public string Key { get; set; }
            public Type FieldType { get; set; }
        }

        /// <summary>
        /// 属性映射信息
        /// </summary>
        private class PropertyMappingInfo
        {
            public PropertyInfo PropertyInfo { get; set; }
            public string Key { get; set; }
            public Type PropertyType { get; set; }
        }
    }
}