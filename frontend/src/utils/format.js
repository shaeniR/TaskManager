export const STATUSES = [
  { value: 'ToDo', label: 'To Do' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Done', label: 'Done' },
];

export const PRIORITIES = ['Low', 'Medium', 'High'];

export const statusLabel = (s) => STATUSES.find((x) => x.value === s)?.label ?? s;

export function formatDate(value) {
  if (!value) return '—';
  return new Date(value).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
}

// "2026-10-19T00:00:00" -> "2026-10-19" for <input type="date">
export const toDateInput = (value) => (value ? value.slice(0, 10) : '');

export function isOverdue(task) {
  if (!task.dueDate || task.status === 'Done') return false;
  return new Date(task.dueDate) < new Date(new Date().toDateString());
}

export function initials(name = '') {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0].toUpperCase())
    .join('');
}
