import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import Modal from './Modal';
import Avatar from './Avatar';
import { taskApi } from '../api/services';
import { getErrorMessage } from '../api/axios';
import { PRIORITIES, STATUSES, formatDate, toDateInput } from '../utils/format';

// Used for both creating (task = null) and editing a task
export default function TaskModal({ task, projectId, members, canDelete, defaultStatus, onClose, onSaved }) {
  const isEdit = Boolean(task);
  const [form, setForm] = useState({
    title: task?.title ?? '',
    description: task?.description ?? '',
    status: task?.status ?? defaultStatus ?? 'ToDo',
    priority: task?.priority ?? 'Medium',
    dueDate: toDateInput(task?.dueDate),
    assignedToId: task?.assignedTo?.id ?? '',
  });
  const [saving, setSaving] = useState(false);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');

  useEffect(() => {
    if (isEdit) taskApi.getComments(task.id).then(setComments).catch(() => {});
  }, [isEdit, task?.id]);

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    const payload = {
      title: form.title,
      description: form.description || null,
      priority: form.priority,
      dueDate: form.dueDate || null,
      assignedToId: form.assignedToId ? Number(form.assignedToId) : null,
    };
    try {
      if (isEdit) {
        await taskApi.update(task.id, { ...payload, status: form.status });
        toast.success('Task updated');
      } else {
        const created = await taskApi.create({ ...payload, projectId });
        if (form.status !== 'ToDo') await taskApi.updateStatus(created.id, form.status);
        toast.success('Task created');
      }
      onSaved();
    } catch (err) {
      toast.error(getErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Delete this task?')) return;
    try {
      await taskApi.remove(task.id);
      toast.success('Task deleted');
      onSaved();
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;
    try {
      const c = await taskApi.addComment(task.id, newComment);
      setComments([...comments, c]);
      setNewComment('');
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  return (
    <Modal title={isEdit ? 'Edit Task' : 'New Task'} onClose={onClose} width={600}>
      <form onSubmit={handleSubmit} className="form">
        <label>
          Title
          <input value={form.title} onChange={set('title')} required maxLength={200} autoFocus />
        </label>
        <label>
          Description
          <textarea value={form.description} onChange={set('description')} rows={3} maxLength={2000} />
        </label>
        <div className="form-row">
          <label>
            Status
            <select value={form.status} onChange={set('status')}>
              {STATUSES.map((s) => (
                <option key={s.value} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
          </label>
          <label>
            Priority
            <select value={form.priority} onChange={set('priority')}>
              {PRIORITIES.map((p) => (
                <option key={p}>{p}</option>
              ))}
            </select>
          </label>
        </div>
        <div className="form-row">
          <label>
            Assignee
            <select value={form.assignedToId} onChange={set('assignedToId')}>
              <option value="">Unassigned</option>
              {members.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Due date
            <input type="date" value={form.dueDate} onChange={set('dueDate')} />
          </label>
        </div>

        <div className="modal-actions">
          {isEdit && canDelete && (
            <button type="button" className="btn btn-danger-outline" onClick={handleDelete}>
              Delete
            </button>
          )}
          <span className="spacer" />
          <button type="button" className="btn btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create task'}
          </button>
        </div>
      </form>

      {isEdit && (
        <div className="comments">
          <h4>Comments ({comments.length})</h4>
          {comments.map((c) => (
            <div key={c.id} className="comment">
              <Avatar name={c.user.name} size={28} />
              <div>
                <div className="small">
                  <strong>{c.user.name}</strong> <span className="muted">· {formatDate(c.createdAt)}</span>
                </div>
                <p>{c.content}</p>
              </div>
            </div>
          ))}
          <form onSubmit={handleAddComment} className="comment-form">
            <input
              placeholder="Write a comment…"
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              maxLength={1000}
            />
            <button className="btn btn-secondary">Post</button>
          </form>
        </div>
      )}
    </Modal>
  );
}
