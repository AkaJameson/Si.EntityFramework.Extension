using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TableBuilder
{
    private readonly DbContext _context;
    private readonly DynamicTableOptions _options;

    public TableBuilder(DbContext context, DynamicTableOptions options)
    {
        _context = context;
        _options = options;
    }

    public async Task CreateTableAsync(string tableName, List<TableColumn> columns)
    {
        var sql = GenerateCreateTableSql(tableName, columns);
        await _context.Database.ExecuteSqlRawAsync(sql);
    }

    private string GenerateCreateTableSql(string tableName, List<TableColumn> columns)
    {
        var columnDefinitions = columns.Select(c => 
            $"{c.Name} {GetSqlType(c.Type)} {(c.IsNullable ? "NULL" : "NOT NULL")} " +
            $"{(c.IsPrimaryKey ? "PRIMARY KEY" : "")} " +
            $"{(c.IsIdentity ? "IDENTITY(1,1)" : "")}");

        return $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}')
            CREATE TABLE {tableName} (
                {string.Join(",\n", columnDefinitions)}
            )";
    }

    private string GetSqlType(Type type)
    {
        return type.Name.ToLower() switch
        {
            "int32" => "int",
            "string" => "nvarchar(max)",
            "datetime" => "datetime2",
            "decimal" => "decimal(18,2)",
            "bool" => "bit",
            _ => "nvarchar(max)"
        };
    }
}

public class TableColumn
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
} 