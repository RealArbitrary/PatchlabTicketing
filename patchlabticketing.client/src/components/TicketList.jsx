import { Fragment, useEffect, useState } from "react";
import {
  getTickets,
  closeTicket,
  getTicketFeedback,
  getTicketComments,
  addTicketComment,
  deleteTicketComment,
  exportTicketsCsv,
} from "../api/tickets";
import StatusBadge from "./StatusBadge";

const POLL_INTERVAL_MS = 5000;

function TicketList() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [closingId, setClosingId] = useState(null);
  const [closeErrors, setCloseErrors] = useState({});
  const [expandedIds, setExpandedIds] = useState(new Set());
  const [feedbackCache, setFeedbackCache] = useState({});
  const [feedbackLoading, setFeedbackLoading] = useState({});
  const [feedbackError, setFeedbackError] = useState({});
  const [commentCache, setCommentCache] = useState({});
  const [commentLoading, setCommentLoading] = useState({});
  const [commentError, setCommentError] = useState({});
  const [newCommentText, setNewCommentText] = useState({});
  const [savingCommentId, setSavingCommentId] = useState(null);
  const [deletingCommentId, setDeletingCommentId] = useState(null);
  const [exporting, setExporting] = useState(false);
  const [exportError, setExportError] = useState(null);

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

  async function handleClose(ticket) {
    setClosingId(ticket.id);
    setCloseErrors((prev) => ({ ...prev, [ticket.id]: null }));

    try {
      await closeTicket(ticket.ticketNumber);
      setTickets((prev) =>
        prev.map((t) => (t.id === ticket.id ? { ...t, status: "Closed" } : t)),
      );
    } catch {
      setCloseErrors((prev) => ({
        ...prev,
        [ticket.id]: "Failed to close ticket.",
      }));
    } finally {
      setClosingId(null);
    }
  }

  async function fetchFeedbackIfNeeded(ticket) {
    if (feedbackCache[ticket.id]) return;

    setFeedbackLoading((prev) => ({ ...prev, [ticket.id]: true }));
    setFeedbackError((prev) => ({ ...prev, [ticket.id]: null }));

    try {
      const data = await getTicketFeedback(ticket.ticketNumber);
      setFeedbackCache((prev) => ({ ...prev, [ticket.id]: data }));
    } catch {
      setFeedbackError((prev) => ({
        ...prev,
        [ticket.id]: "Could not load feedback.",
      }));
    } finally {
      setFeedbackLoading((prev) => ({ ...prev, [ticket.id]: false }));
    }
  }

  async function fetchCommentsIfNeeded(ticket) {
    if (commentCache[ticket.id]) return;

    setCommentLoading((prev) => ({ ...prev, [ticket.id]: true }));
    setCommentError((prev) => ({ ...prev, [ticket.id]: null }));

    try {
      const data = await getTicketComments(ticket.ticketNumber);
      setCommentCache((prev) => ({ ...prev, [ticket.id]: data }));
    } catch {
      setCommentError((prev) => ({
        ...prev,
        [ticket.id]: "Could not load comments.",
      }));
    } finally {
      setCommentLoading((prev) => ({ ...prev, [ticket.id]: false }));
    }
  }

  async function handleAddComment(ticket) {
    const text = (newCommentText[ticket.id] || "").trim();
    if (!text) return;

    setSavingCommentId(ticket.id);
    setCommentError((prev) => ({ ...prev, [ticket.id]: null }));

    try {
      await addTicketComment(ticket.ticketNumber, text);
      const data = await getTicketComments(ticket.ticketNumber);
      setCommentCache((prev) => ({ ...prev, [ticket.id]: data }));
      setNewCommentText((prev) => ({ ...prev, [ticket.id]: "" }));
    } catch {
      setCommentError((prev) => ({
        ...prev,
        [ticket.id]: "Failed to save comment.",
      }));
    } finally {
      setSavingCommentId(null);
    }
  }

  async function handleDeleteComment(ticket, comment) {
    if (!window.confirm("Delete this comment?")) return;

    setDeletingCommentId(comment.id);
    setCommentError((prev) => ({ ...prev, [ticket.id]: null }));

    try {
      await deleteTicketComment(ticket.ticketNumber, comment.id);
      setCommentCache((prev) => ({
        ...prev,
        [ticket.id]: prev[ticket.id].filter((c) => c.id !== comment.id),
      }));
    } catch {
      setCommentError((prev) => ({
        ...prev,
        [ticket.id]: "Failed to delete comment.",
      }));
    } finally {
      setDeletingCommentId(null);
    }
  }

  async function handleExportCsv() {
    setExporting(true);
    setExportError(null);

    try {
      const blob = await exportTicketsCsv();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = "tickets-export.csv";
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch {
      setExportError("Failed to export tickets.");
    } finally {
      setExporting(false);
    }
  }

  function handleRowClick(ticket) {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(ticket.id)) {
        next.delete(ticket.id);
      } else {
        next.add(ticket.id);
      }
      return next;
    });

    if (!expandedIds.has(ticket.id)) {
      fetchFeedbackIfNeeded(ticket);
      fetchCommentsIfNeeded(ticket);
    }
  }

  function handleExpandAll() {
    setExpandedIds(new Set(tickets.map((t) => t.id)));
    tickets.forEach((ticket) => {
      fetchFeedbackIfNeeded(ticket);
      fetchCommentsIfNeeded(ticket);
    });
  }

  function handleCollapseAll() {
    setExpandedIds(new Set());
  }

  if (loading) {
    return <p className="ticket-status-message">Loading tickets...</p>;
  }

  if (error) {
    return <p className="ticket-status-message ticket-status-error">{error}</p>;
  }

  if (tickets.length === 0) {
    return <p className="ticket-status-message">No tickets yet.</p>;
  }

  const allExpanded =
    tickets.length > 0 && tickets.every((t) => expandedIds.has(t.id));

  return (
    <div>
      <div className="ticket-list-toolbar">
        <button
          className="toolbar-btn"
          onClick={allExpanded ? handleCollapseAll : handleExpandAll}
        >
          {allExpanded ? "Collapse all" : "Expand all"}
        </button>
        <button
          className="toolbar-btn"
          onClick={handleExportCsv}
          disabled={exporting}
        >
          {exporting ? "Exporting..." : "Export to CSV"}
        </button>
        {exportError && (
          <span className="ticket-status-message ticket-status-error">
            {exportError}
          </span>
        )}
      </div>

      <table className="ticket-table">
        <thead>
          <tr>
            <th>Ticket</th>
            <th>Name</th>
            <th>Cellphone</th>
            <th>Issue</th>
            <th>Created</th>
            <th>Area</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {tickets.map((ticket) => (
            <Fragment key={ticket.id}>
              <tr
                className="ticket-row-clickable"
                onClick={() => handleRowClick(ticket)}
              >
                <td>{ticket.ticketNumber}</td>
                <td>
                  {`${ticket.firstName ?? ""} ${ticket.lastName ?? ""}`.trim() ||
                    "—"}
                </td>
                <td>{ticket.cellphoneNumber}</td>
                <td>{ticket.issue}</td>
                <td>{new Date(ticket.createdAt).toLocaleString()}</td>
                <td>{ticket.area || "—"}</td>
                <td>
                  <StatusBadge status={ticket.status} />
                </td>
                <td onClick={(e) => e.stopPropagation()}>
                  <button className="action-btn" disabled title="Coming soon">
                    Open Chat
                  </button>
                  <button
                    className="action-btn action-btn-active"
                    onClick={() => handleClose(ticket)}
                    disabled={
                      ticket.status === "Closed" || closingId === ticket.id
                    }
                  >
                    {closingId === ticket.id ? "Closing..." : "Close"}
                  </button>
                  {closeErrors[ticket.id] && (
                    <div className="action-error">{closeErrors[ticket.id]}</div>
                  )}
                </td>
              </tr>
              {expandedIds.has(ticket.id) && (
                <tr className="ticket-feedback-row">
                  <td colSpan={8}>
                    {feedbackLoading[ticket.id] && (
                      <p className="ticket-status-message">
                        Loading feedback...
                      </p>
                    )}
                    {feedbackError[ticket.id] && (
                      <p className="ticket-status-message ticket-status-error">
                        {feedbackError[ticket.id]}
                      </p>
                    )}
                    {feedbackCache[ticket.id] &&
                      (feedbackCache[ticket.id].length === 0 ? (
                        <p className="ticket-status-message">
                          No feedback yet.
                        </p>
                      ) : (
                        <ul className="feedback-list">
                          {feedbackCache[ticket.id].map((f) => (
                            <li key={f.id} className="feedback-item">
                              <span
                                className={
                                  f.status === "Unhappy"
                                    ? "feedback-status-unhappy"
                                    : "feedback-status-satisfied"
                                }
                              >
                                {f.status}
                              </span>
                              {f.reason && (
                                <span className="feedback-reason">
                                  {" "}
                                  — {f.reason}
                                </span>
                              )}
                              <span className="feedback-date">
                                {new Date(f.createdAt).toLocaleString()}
                              </span>
                            </li>
                          ))}
                        </ul>
                      ))}

                    <div className="comments-section">
                      <h4 className="comments-heading">Comments</h4>

                      {commentLoading[ticket.id] && (
                        <p className="ticket-status-message">
                          Loading comments...
                        </p>
                      )}
                      {commentError[ticket.id] && (
                        <p className="ticket-status-message ticket-status-error">
                          {commentError[ticket.id]}
                        </p>
                      )}
                      {commentCache[ticket.id] &&
                        (commentCache[ticket.id].length === 0 ? (
                          <p className="ticket-status-message">
                            No comments yet.
                          </p>
                        ) : (
                          <ul className="comment-list">
                            {commentCache[ticket.id].map((c) => (
                              <li key={c.id} className="comment-item">
                                <span className="comment-text">
                                  {c.comment}
                                </span>
                                <span className="comment-meta">
                                  <span className="comment-date">
                                    {new Date(c.createdAt).toLocaleString()}
                                  </span>
                                  <button
                                    className="comment-delete-btn"
                                    onClick={() =>
                                      handleDeleteComment(ticket, c)
                                    }
                                    disabled={deletingCommentId === c.id}
                                    title="Delete comment"
                                  >
                                    {deletingCommentId === c.id ? "…" : "✕"}
                                  </button>
                                </span>
                              </li>
                            ))}
                          </ul>
                        ))}

                      <div
                        className="comment-input-row"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <input
                          type="text"
                          className="comment-input"
                          placeholder="Add a comment..."
                          value={newCommentText[ticket.id] || ""}
                          onChange={(e) =>
                            setNewCommentText((prev) => ({
                              ...prev,
                              [ticket.id]: e.target.value,
                            }))
                          }
                        />
                        <button
                          className="action-btn action-btn-active"
                          onClick={() => handleAddComment(ticket)}
                          disabled={
                            savingCommentId === ticket.id ||
                            !(newCommentText[ticket.id] || "").trim()
                          }
                        >
                          {savingCommentId === ticket.id ? "Saving..." : "Save"}
                        </button>
                      </div>
                    </div>
                  </td>
                </tr>
              )}
            </Fragment>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default TicketList;
