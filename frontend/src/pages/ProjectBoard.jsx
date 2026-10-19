import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import TaskCard from '../components/TaskCard';
import TaskModal from '../components/TaskModal';
import MembersModal from '../components/MembersModal';
import Avatar from '../components/Avatar';
import { projectApi, taskApi } from '../api/services';
import { getErrorMessage } from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { PRIORITIES, STATUSES, formatDate, isOverdue, statusLabel } from '../utils/format';

const LIST_PAGE_SIZE = 8;

export default function ProjectBoard() {
  const { id } = useParams();
  const projectId = Number(id);
  const navigate = useNavigate();
  const { user, isAdmin } = useAuth();

  const [project, setProject] = useState(null);
  const [tasks, setTasks] = useState({ items: [], totalCount: 0, totalPages: 0 });
  const [view, setView] = useState('board');
  const [filters, setFilters] = useState({ search: '', priority: '', assignedToId: '', status: '' });
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState(null); // { task, status } | 'members'

  const isOwner = project && (project.owner.id === user.id || isAdmin);

  const loadProject = useCallback(
    () =>
      projectApi.get(projectId).then(setProject).catch((err) => {
        toast.error(getErrorMessage(err));
        navigate('/projects');
      }),
    [projectId, navigate]
  );

  const loadTasks = useCallback(() => {
    const params = {
      search: debouncedSearch || undefined,
      priority: filters.priority || undefined,
      assignedToId: filters.assignedToId || undefined,
      // Board shows all statuses in columns; list view supports status filter + pagination
      status: view === 'list' ? filters.status || undefined : undefined,
      page: view === 'list' ? page : 1,
      pageSize: view === 'list' ? LIST_PAGE_SIZE : 100,
    };
    return projectApi.getTasks(projectId, params).then(setTasks).catch((err) => toast.error(getErrorMessage(err)));
  }, [projectId, debouncedSearch, filters.priority, filters.assignedToId, filters.status, view, page]);

  useEffect(() => {
    loadProject();
  }, [loadProject]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  // Debounce the search box so we don't call the API on every keystroke
  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(filters.search), 350);
    return () => clearTimeout(t);
  }, [filters.search]);

  const setFilter = (field) => (e) => {
    setFilters({ ...filters, [field]: e.target.value });
    setPage(1);
  };

  // ----- Drag & drop between columns -----
  const onDragStart = (e, task) => e.dataTransfer.setData('taskId', String(task.id));

  const onDrop = async (e, status) => {
    e.preventDefault();
    e.currentTarget.classList.remove('drag-over');
    const taskId = Number(e.dataTransfer.getData('taskId'));
    const task = tasks.items.find((t) => t.id === taskId);
    if (!task || task.status === status) return;

    // Optimistic update: move the card immediately, roll back if the API fails
    const previous = tasks;
    setTasks({ ...tasks, items: tasks.items.map((t) => (t.id === taskId ? { ...t, status } : t)) });
    try {
      await taskApi.updateStatus(taskId, status);
      toast.success(`Moved to ${statusLabel(status)}`);
    } catch (err) {
      setTasks(previous);
      toast.error(getErrorMessage(err));
    }
  };

  const handleDeleteProject = async () => {
    if (!window.confirm(`Delete project "${project.name}" and all its tasks?`)) return;
    try {
      await projectApi.remove(projectId);
      toast.success('Project deleted');
      navigate('/projects');
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  const onSaved = () => {
    setModal(null);
    loadTasks();
  };

  if (!project) return <div className="loading">Loading project…</div>;

  return (
    <div>
      <header className="page-header">
        <div>
          <button className="link-back" onClick={() => navigate('/projects')}>
            ← Projects
          </button>
          <h1>{project.name}</h1>
          {project.description && <p className="muted">{project.description}</p>}
        </div>
        <div className="header-actions">
          <button className="avatar-stack" onClick={() => setModal('members')} title="Members">
            {project.members.slice(0, 4).map((m) => (
              <Avatar key={m.id} name={m.name} size={30} />
            ))}
            {project.members.length > 4 && <span className="more">+{project.members.length - 4}</span>}
          </button>
          {isOwner && (
            <button className="btn btn-danger-outline" onClick={handleDeleteProject}>
              Delete
            </button>
          )}
          <button className="btn btn-primary" onClick={() => setModal({ task: null })}>
            + New task
          </button>
        </div>
      </header>

      <div className="toolbar">
        <input
          className="input search"
          placeholder="🔍 Search tasks…"
          value={filters.search}
          onChange={setFilter('search')}
        />
        <select className="input" value={filters.priority} onChange={setFilter('priority')}>
          <option value="">All priorities</option>
          {PRIORITIES.map((p) => (
            <option key={p}>{p}</option>
          ))}
        </select>
        <select className="input" value={filters.assignedToId} onChange={setFilter('assignedToId')}>
          <option value="">All assignees</option>
          {project.members.map((m) => (
            <option key={m.id} value={m.id}>
              {m.name}
            </option>
          ))}
        </select>
        {view === 'list' && (
          <select className="input" value={filters.status} onChange={setFilter('status')}>
            <option value="">All statuses</option>
            {STATUSES.map((s) => (
              <option key={s.value} value={s.value}>
                {s.label}
              </option>
            ))}
          </select>
        )}
        <span className="spacer" />
        <div className="view-toggle">
          <button className={view === 'board' ? 'active' : ''} onClick={() => setView('board')}>
            Board
          </button>
          <button
            className={view === 'list' ? 'active' : ''}
            onClick={() => {
              setView('list');
              setPage(1);
            }}
          >
            List
          </button>
        </div>
      </div>

      {view === 'board' ? (
        <div className="board">
          {STATUSES.map((col) => {
            const colTasks = tasks.items.filter((t) => t.status === col.value);
            return (
              <div
                key={col.value}
                className={`column column-${col.value.toLowerCase()}`}
                onDragOver={(e) => {
                  e.preventDefault();
                  e.currentTarget.classList.add('drag-over');
                }}
                onDragLeave={(e) => e.currentTarget.classList.remove('drag-over')}
                onDrop={(e) => onDrop(e, col.value)}
              >
                <div className="column-header">
                  <span>{col.label}</span>
                  <span className="count">{colTasks.length}</span>
                </div>
                <div className="column-body">
                  {colTasks.map((t) => (
                    <TaskCard key={t.id} task={t} onDragStart={onDragStart} onClick={(task) => setModal({ task })} />
                  ))}
                  <button className="add-task" onClick={() => setModal({ task: null, status: col.value })}>
                    + Add task
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="panel">
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Assignee</th>
                <th>Due</th>
              </tr>
            </thead>
            <tbody>
              {tasks.items.map((t) => (
                <tr key={t.id} onClick={() => setModal({ task: t })}>
                  <td>{t.title}</td>
                  <td>
                    <span className={`badge status-${t.status.toLowerCase()}`}>{statusLabel(t.status)}</span>
                  </td>
                  <td>
                    <span className={`badge priority-${t.priority.toLowerCase()}`}>{t.priority}</span>
                  </td>
                  <td>{t.assignedTo?.name ?? <span className="muted">—</span>}</td>
                  <td className={isOverdue(t) ? 'text-danger' : ''}>{formatDate(t.dueDate)}</td>
                </tr>
              ))}
              {tasks.items.length === 0 && (
                <tr>
                  <td colSpan={5} className="muted center">
                    No tasks found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
          <div className="pagination">
            <span className="muted small">
              {tasks.totalCount} task{tasks.totalCount === 1 ? '' : 's'}
            </span>
            <span className="spacer" />
            <button className="btn btn-sm btn-secondary" disabled={page <= 1} onClick={() => setPage(page - 1)}>
              ‹ Prev
            </button>
            <span className="small">
              Page {page} of {Math.max(1, tasks.totalPages)}
            </span>
            <button
              className="btn btn-sm btn-secondary"
              disabled={page >= tasks.totalPages}
              onClick={() => setPage(page + 1)}
            >
              Next ›
            </button>
          </div>
        </div>
      )}

      {modal === 'members' && (
        <MembersModal
          project={project}
          canManage={isOwner}
          onClose={() => setModal(null)}
          onChanged={() => {
            loadProject();
            loadTasks();
          }}
        />
      )}
      {modal && modal !== 'members' && (
        <TaskModal
          task={modal.task}
          defaultStatus={modal.status}
          projectId={projectId}
          members={project.members}
          canDelete={isOwner}
          onClose={() => setModal(null)}
          onSaved={onSaved}
        />
      )}
    </div>
  );
}
