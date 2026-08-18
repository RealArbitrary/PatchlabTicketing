import axios from "axios";

const API_BASE_URL = "https://localhost:7168/api";

export async function getTickets() {
  const response = await axios.get(`${API_BASE_URL}/Tickets`);
  return response.data;
}
