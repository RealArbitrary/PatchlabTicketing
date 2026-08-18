import { useState } from "react";

function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);

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
          <a href="/" className="side-menu-link active">
            Tickets
          </a>
          <a href="#" className="side-menu-link disabled">
            Conversations
          </a>
          <a href="#" className="side-menu-link disabled">
            Settings
          </a>
        </nav>
      )}
    </header>
  );
}

export default Navbar;
