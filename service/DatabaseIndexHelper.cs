using SqlSugar;

namespace dy.net.service
{
    /// <summary>
    /// Provides database-specific index metadata checks used by startup schema initialization.
    /// </summary>
    internal static class DatabaseIndexHelper
    {
        public static bool MySqlIndexExists(
            ISqlSugarClient db,
            string tableName,
            string indexName)
        {
            // The values passed here are application-owned schema identifiers. Escaping them
            // still keeps this helper safe if a future identifier contains a single quote.
            var escapedTableName = tableName.Replace("'", "''");
            var escapedIndexName = indexName.Replace("'", "''");
            var sql = $@"SELECT COUNT(1)
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = '{escapedTableName}'
                  AND INDEX_NAME = '{escapedIndexName}'";

            return db.Ado.SqlQuery<long>(sql).FirstOrDefault() > 0;
        }
    }
}
