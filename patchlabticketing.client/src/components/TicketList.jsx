import { useEffect, useState } from "react";
import { getTickets } from "../api/tickets";
import StatusBadge from "./StatusBadge";

const POLL_INTERVAL_MS = 5000;

function TicketList() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let isMounted = true;

    async function fetchTickets() {
      try {
        const data = await getTickets();
        if (isMounted) {
          setTickets(data);
          setError(null);
        }
      } catch {
        if (isMounted) {
          setError("Could not load tickets.");
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    }

    fetchTickets();
    const intervalId = setInterval(fetchTickets, POLL_INTERVAL_MS);

    return () => {
      isMounted = false;
      clearInterval(intervalId);
    };
  }, []);

  if (loading) {
    return <p className="ticket-status-message">Loading tickets...</p>;
  }

  if (error) {
    return <p className="ticket-status-message ticket-status-error">{error}</p>;
  }

  if (tickets.length === 0) {
    return <p className="ticket-status-message">No tickets yet.</p>;
  }

  return (
    <table className="ticket-table">
      <thead>
        <tr>
          <th>Ticket</th>
          <th>Cellphone</th>
          <th>Issue</th>
          <th>Created</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {tickets.map((ticket) => (
          <tr key={ticket.id}>
            <td>{ticket.ticketNumber}</td>
            <td>{ticket.cellphoneNumber}</td>
            <td>{ticket.issue}</td>
            <td>{new Date(ticket.createdAt).toLocaleString()}</td>
            <td>
              <StatusBadge status={ticket.status} />
            </td>
            <td>
              <button className="action-btn" disabled title="Coming soon">
                Open Chat
              </button>
              <button className="action-btn" disabled title="Coming soon">
                Close
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default TicketList;
