import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { dashboardApi } from '../api/services';
import { getErrorMessage } from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { formatDate, isOverdue, statusLabel } from '../utils/format';

export default function Dashboard() {
  const { user } = useAuth();
  const [data, setData] = useState(null);

  useEffect(() => {
    dashboardApi.get().then(setData).catch((err) => toast.error(getErrorMessage(err)));
  }, []);

  if (!data) return <div className="loading">Loading dashboard…</div>;

  const total = data.totalTasks || 1;
  const bars = [
    { label: 'To Do', value: data.toDo, cls: 'todo' },
    { label: 'In Progress', value: data.inProgress, cls: 'inprogress' },
    { label: 'Done', value: data.done, cls: 'done' },
  ];
  const completion = data.totalTasks ? Math.round((data.done / data.totalTasks) * 100) : 0;

  return (
    <div>
      <header className="page-header">
        <div>
          <h1>Hello, {user.name.split(' ')[0]} 👋</h1>
          <p className="muted">Here's what's happening across your projects.</p>
        </div>
      </header>

      <div className="stats-grid">
        <StatCard label="Projects" value={data.totalProjects} icon="📁" />
        <StatCard label="Total tasks" value={data.totalTasks} icon="🗂️" />
        <StatCard label="My open tasks" value={data.myOpenTasks} icon="🙋" />
        <StatCard label="Overdue" value={data.overdue} icon="⏰" danger={data.overdue > 0} />
      </div>

      <div className="dash-grid">
        <section className="panel">
          <h3>Tasks by status</h3>
          <div className="completion">
            <span className="completion-value">{completion}%</span>
            <span className="muted">completed</span>
          </div>
          {bars.map((b) => (
            <div key={b.label} className="bar-row">
              <span className="bar-label">{b.label}</span>
              <div className="bar-track">
                <div className={`bar-fill ${b.cls}`} style={{ width: `${(b.value / total) * 100}%` }} />
              </div>
              <span className="bar-value">{b.value}</span>
            </div>
          ))}
        </section>

        <section className="panel">
          <h3>My upcoming tasks</h3>
          {data.upcomingTasks.length === 0 && <p className="muted">Nothing assigned to you. 🎉</p>}
          <ul className="upcoming">
            {data.upcomingTasks.map((t) => (
              <li key={t.id}>
                <Link to={`/projects/${t.projectId}`}>
                  <div>
                    <strong>{t.title}</strong>
                    <div className="muted small">
                      {t.projectName} · {statusLabel(t.status)}
                    </div>
                  </div>
                  <span className={`small ${isOverdue(t) ? 'text-danger' : 'muted'}`}>{formatDate(t.dueDate)}</span>
                </Link>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}

function StatCard({ label, value, icon, danger }) {
  return (
    <div className={`stat-card ${danger ? 'stat-danger' : ''}`}>
      <span className="stat-icon">{icon}</span>
      <div>
        <div className="stat-value">{value}</div>
        <div className="muted small">{label}</div>
      </div>
    </div>
  );
}
