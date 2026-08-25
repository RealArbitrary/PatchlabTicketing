function StatusBadge({ status }) {
  const normalized = (status || "").toLowerCase();
  const className = `status-text status-text-${normalized}`;

  return <span className={className}>{status}</span>;
}

export default StatusBadge;
