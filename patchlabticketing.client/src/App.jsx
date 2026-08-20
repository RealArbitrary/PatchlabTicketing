import { useState } from "react";
import Navbar from "./components/Navbar";
import TicketList from "./components/TicketList";
import ErrorLogList from "./components/ErrorLogList";
import "./styles/Navbar.css";
import "./styles/TicketList.css";
import "./styles/ErrorLogList.css";

function App() {
  const [view, setView] = useState("tickets");

  return (
    <div>
      <Navbar activeView={view} onNavigate={setView} />
      {view === "tickets" ? <TicketList /> : <ErrorLogList />}
    </div>
  );
}

export default App;
