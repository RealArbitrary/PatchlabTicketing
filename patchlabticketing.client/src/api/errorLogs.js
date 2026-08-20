import axios from "axios";

const API_BASE_URL = "/api";

export async function getErrorLogs() {
  const response = await axios.get(`${API_BASE_URL}/ErrorLogs`);
  return response.data;
}
