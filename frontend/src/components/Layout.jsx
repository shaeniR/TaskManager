import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Avatar from './Avatar';

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-logo">✓</span>
          <span>TaskFlow</span>
        </div>
        <nav>
          <NavLink to="/" end>
            <span>📊</span> Dashboard
          </NavLink>
          <NavLink to="/projects">
            <span>📁</span> Projects
          </NavLink>
        </nav>
        <div className="sidebar-user">
          <Avatar name={user.name} />
          <div className="sidebar-user-info">
            <strong>{user.name}</strong>
            <small>{user.role}</small>
          </div>
          <button className="btn-icon" title="Logout" onClick={handleLogout}>
            ⎋
          </button>
        </div>
      </aside>
      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}
