import { useEffect, useState } from "react";
import { getErrorLogs } from "../api/errorLogs";

const POLL_INTERVAL_MS = 10000;

function ErrorLogList() {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let isMounted = true;

    async function fetchLogs() {
      try {
        const data = await getErrorLogs();
        if (isMounted) {
          setLogs(data);
          setError(null);
        }
      } catch {
        if (isMounted) {
          setError("Could not load error logs.");
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    }

    fetchLogs();
    const intervalId = setInterval(fetchLogs, POLL_INTERVAL_MS);

    return () => {
      isMounted = false;
      clearInterval(intervalId);
    };
  }, []);

  if (loading) {
    return <p className="ticket-status-message">Loading error logs...</p>;
  }

  if (error) {
    return <p className="ticket-status-message ticket-status-error">{error}</p>;
  }

  if (logs.length === 0) {
    return (
      <p className="ticket-status-message">No errors logged. All quiet.</p>
    );
  }

  return (
    <table className="ticket-table error-log-table">
      <thead>
        <tr>
          <th>Severity</th>
          <th>Source</th>
          <th>Message</th>
          <th>Created</th>
        </tr>
      </thead>
      <tbody>
        {logs.map((log) => (
          <tr key={log.id}>
            <td>
              <span
                className={
                  log.severity === "Critical" || log.severity === "Error"
                    ? "severity-badge severity-error"
                    : "severity-badge severity-warning"
                }
              >
                {log.severity}
              </span>
            </td>
            <td className="error-log-source">{log.source}</td>
            <td className="error-log-message" title={log.stackTrace || ""}>
              {log.message}
            </td>
            <td>{new Date(log.createdAt).toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default ErrorLogList;
