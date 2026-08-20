import { useState } from "react";

function Navbar({ activeView, onNavigate }) {
  const [menuOpen, setMenuOpen] = useState(false);

  function handleNavClick(view) {
    onNavigate(view);
    setMenuOpen(false);
  }

  return (
    <header className="navbar">
      <div className="navbar-left">
        <button
          className="burger-btn"
          onClick={() => setMenuOpen(!menuOpen)}
          aria-label="Toggle menu"
        >
          <span />
          <span />
          <span />
        </button>
        <span className="navbar-title">
          Patchlab<span className="navbar-title-accent">Ticketing</span>
        </span>
      </div>

      {menuOpen && (
        <nav className="side-menu">
          <a
            href="#"
            className={`side-menu-link ${activeView === "tickets" ? "active" : ""}`}
            onClick={(e) => {
              e.preventDefault();
              handleNavClick("tickets");
            }}
          >
            Tickets
          </a>
          <a href="#" className="side-menu-link disabled">
            Conversations
          </a>
          <a
            href="#"
            className={`side-menu-link ${activeView === "errors" ? "active" : ""}`}
            onClick={(e) => {
              e.preventDefault();
              handleNavClick("errors");
            }}
          >
            Error Logs
          </a>
        </nav>
      )}
    </header>
  );
}

export default Navbar;
