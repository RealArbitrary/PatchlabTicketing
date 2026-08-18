function StatusBadge({ status }) {
  const normalized = (status || "").toLowerCase();
  const className = `status-badge status-badge-${normalized}`;

  return <span className={className}>{status}</span>;
}

export default StatusBadge;
