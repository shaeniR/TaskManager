import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import Modal from './Modal';
import Avatar from './Avatar';
import { projectApi, userApi } from '../api/services';
import { getErrorMessage } from '../api/axios';

export default function MembersModal({ project, canManage, onClose, onChanged }) {
  const [search, setSearch] = useState('');
  const [results, setResults] = useState([]);

  // Debounced user search
  useEffect(() => {
    if (!canManage) return;
    const id = setTimeout(() => {
      userApi.search(search).then(setResults).catch(() => {});
    }, 300);
    return () => clearTimeout(id);
  }, [search, canManage]);

  const memberIds = new Set(project.members.map((m) => m.id));
  const candidates = results.filter((u) => !memberIds.has(u.id));

  const add = async (userId) => {
    try {
      await projectApi.addMember(project.id, userId);
      toast.success('Member added');
      onChanged();
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  const remove = async (userId) => {
    if (!window.confirm('Remove this member? Their tasks will be unassigned.')) return;
    try {
      await projectApi.removeMember(project.id, userId);
      toast.success('Member removed');
      onChanged();
    } catch (err) {
      toast.error(getErrorMessage(err));
    }
  };

  return (
    <Modal title="Project Members" onClose={onClose}>
      <ul className="member-list">
        {project.members.map((m) => (
          <li key={m.id}>
            <Avatar name={m.name} />
            <div className="grow">
              <strong>{m.name}</strong>
              <div className="muted small">{m.email}</div>
            </div>
            {m.id === project.owner.id ? (
              <span className="badge">Owner</span>
            ) : (
              canManage && (
                <button className="btn btn-sm btn-danger-outline" onClick={() => remove(m.id)}>
                  Remove
                </button>
              )
            )}
          </li>
        ))}
      </ul>

      {canManage && (
        <>
          <h4 className="mt">Add member</h4>
          <input
            className="input"
            placeholder="Search by name or email…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <ul className="member-list">
            {candidates.map((u) => (
              <li key={u.id}>
                <Avatar name={u.name} />
                <div className="grow">
                  <strong>{u.name}</strong>
                  <div className="muted small">{u.email}</div>
                </div>
                <button className="btn btn-sm btn-primary" onClick={() => add(u.id)}>
                  Add
                </button>
              </li>
            ))}
            {candidates.length === 0 && <li className="muted small">No users to add.</li>}
          </ul>
        </>
      )}
    </Modal>
  );
}
