internal static class RunRecordSql
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS run_records
        (
            request_id TEXT PRIMARY KEY,
            executed_at_utc TEXT NOT NULL,
            runtime_name TEXT NOT NULL,
            model_name TEXT NOT NULL,
            quantization TEXT NULL,
            total_latency_ms INTEGER NOT NULL,
            load_latency_ms INTEGER NOT NULL,
            prompt_latency_ms INTEGER NOT NULL,
            generation_latency_ms INTEGER NOT NULL,
            prompt_tokens INTEGER NOT NULL,
            completion_tokens INTEGER NOT NULL,
            is_streaming INTEGER NOT NULL,
            is_success INTEGER NOT NULL,
            error_category TEXT NULL
        );

        CREATE INDEX IF NOT EXISTS idx_run_records_executed_at_utc
            ON run_records(executed_at_utc DESC);

        CREATE INDEX IF NOT EXISTS idx_run_records_is_success
            ON run_records(is_success);
        """;

    public const string InsertCompleted = """
        INSERT INTO run_records (
            request_id,
            executed_at_utc,
            runtime_name,
            model_name,
            quantization,
            total_latency_ms,
            load_latency_ms,
            prompt_latency_ms,
            generation_latency_ms,
            prompt_tokens,
            completion_tokens,
            is_streaming,
            is_success,
            error_category)
        VALUES (
            $request_id,
            $executed_at_utc,
            $runtime_name,
            $model_name,
            $quantization,
            $total_latency_ms,
            $load_latency_ms,
            $prompt_latency_ms,
            $generation_latency_ms,
            $prompt_tokens,
            $completion_tokens,
            $is_streaming,
            $is_success,
            $error_category);
        """;



    public const string SelectByRequestId = """
        SELECT *
        FROM run_records
        WHERE request_id = $request_id;
        """;

    public const string SelectRecent = """
        SELECT *
        FROM run_records
        ORDER BY executed_at_utc DESC
        LIMIT $limit;
        """;

    public const string SelectSummary = """
        SELECT
          COUNT(*) AS total_runs,
          SUM(CASE WHEN is_success = 1 THEN 1 ELSE 0 END) AS successful_runs,
          SUM(CASE WHEN is_success = 0 THEN 1 ELSE 0 END) AS failed_runs,
          AVG(total_latency_ms) AS average_total_latency_ms
        FROM run_records;
        """;
}