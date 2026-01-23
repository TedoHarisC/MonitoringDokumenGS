using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/debug/schema")]
    public class DebugSchemaController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _env;

        public DebugSchemaController(ApplicationDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogsSchema()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var connectionString = _context.Database.GetDbConnection().ConnectionString;
            await using var connection = new SqlConnection(connectionString);
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                    SELECT 
                    c.ORDINAL_POSITION AS OrdinalPosition,
                    c.COLUMN_NAME AS ColumnName,
                    c.DATA_TYPE AS DataType,
                    c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                    c.NUMERIC_PRECISION AS NumericPrecision,
                    c.NUMERIC_SCALE AS NumericScale,
                    c.IS_NULLABLE AS IsNullable
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    WHERE c.TABLE_NAME = 'AuditLogs'
                    ORDER BY c.ORDINAL_POSITION;";

            var rows = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    ordinalPosition = reader.GetInt32(0),
                    columnName = reader.GetString(1),
                    dataType = reader.GetString(2),
                    maxLength = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                    numericPrecision = reader.IsDBNull(4) ? (byte?)null : reader.GetByte(4),
                    numericScale = reader.IsDBNull(5) ? (int?)null : Convert.ToInt32(reader.GetValue(5)),
                    isNullable = reader.GetString(6)
                });
            }

            return Ok(new { table = "AuditLogs", columns = rows });
        }
    }
}
