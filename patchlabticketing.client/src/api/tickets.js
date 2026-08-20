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
