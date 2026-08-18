import axios from "axios";

const API_BASE_URL = "/api";

export async function getTickets() {
  const response = await axios.get(`${API_BASE_URL}/Tickets`);
  return response.data;
}
