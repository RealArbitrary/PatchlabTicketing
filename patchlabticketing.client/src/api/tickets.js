import axios from "axios";

const API_BASE_URL = "/api";

export async function getTickets() {
  const response = await axios.get(`${API_BASE_URL}/Tickets`);
  return response.data;
}

export async function closeTicket(ticketNumber) {
  await axios.put(`${API_BASE_URL}/Tickets/${ticketNumber}/close`);
}

export async function getTicketFeedback(ticketNumber) {
  const response = await axios.get(
    `${API_BASE_URL}/Tickets/${ticketNumber}/feedback`,
  );
  return response.data;
}

export async function getTicketComments(ticketNumber) {
  const response = await axios.get(
    `${API_BASE_URL}/Tickets/${ticketNumber}/comments`,
  );
  return response.data;
}

export async function addTicketComment(ticketNumber, comment) {
  await axios.post(`${API_BASE_URL}/Tickets/${ticketNumber}/comments`, {
    comment,
  });
}

export async function deleteTicketComment(ticketNumber, commentId) {
  await axios.delete(
    `${API_BASE_URL}/Tickets/${ticketNumber}/comments/${commentId}`,
  );
}

export async function deleteTicket(id) {
  await axios.delete(`${API_BASE_URL}/Tickets/${id}`);
}
