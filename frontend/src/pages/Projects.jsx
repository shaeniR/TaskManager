import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import Modal from '../components/Modal';
import Avatar from '../components/Avatar';
import { projectApi } from '../api/services';
import { getErrorMessage } from '../api/axios';

export default function Projects() {
  const [projects, setProjects] = useState(null);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: '', description: '' });

  const load = () =>
    projectApi
      .getAll()
      .then(setProjects)
      .catch((err) => toast.error(getErrorMessage(err)));

  useEffect(() => {
    load();
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      await projectApi.create(form);
      toast.success('Project created');
      setShowCreate(false);
      setForm({ name: '', description: '' });
      load();
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  return (
    <div>
      <header className="page-header">
        <div>
          <h1>Projects</h1>
          <p className="muted">All projects you are a member of.</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          + New project
        </button>
      </header>

      {!projects && <div className="loading">Loading…</div>}
      {projects?.length === 0 && (
        <div className="empty">
          <p>No projects yet.</p>
          <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
            Create your first project
          </button>
        </div>
      )}

      <div className="project-grid">
        {projects?.map((p) => {
          const progress = p.taskCount ? Math.round((p.doneCount / p.taskCount) * 100) : 0;
          return (
            <Link key={p.id} to={`/projects/${p.id}`} className="project-card">
              <h3>{p.name}</h3>
              <p className="muted project-desc">{p.description || 'No description'}</p>
              <div className="progress">
                <div className="progress-fill" style={{ width: `${progress}%` }} />
              </div>
              <div className="project-meta small muted">
                <span>
                  {p.doneCount}/{p.taskCount} tasks done
                </span>
                <span>👥 {p.memberCount}</span>
              </div>
              <div className="project-owner small">
                <Avatar name={p.owner.name} size={22} /> <span className="muted">{p.owner.name}</span>
              </div>
            </Link>
          );
        })}
      </div>

      {showCreate && (
        <Modal title="New Project" onClose={() => setShowCreate(false)}>
          <form onSubmit={handleCreate} className="form">
            <label>
              Name
              <input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
                maxLength={150}
                autoFocus
              />
            </label>
            <label>
              Description
              <textarea
                rows={3}
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                maxLength={1000}
              />
            </label>
            <div className="modal-actions">
              <span className="spacer" />
              <button type="button" className="btn btn-secondary" onClick={() => setShowCreate(false)}>
                Cancel
              </button>
              <button className="btn btn-primary">Create</button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
