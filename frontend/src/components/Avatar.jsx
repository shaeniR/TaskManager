import { initials } from '../utils/format';

const COLORS = ['#6366f1', '#0ea5e9', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

export default function Avatar({ name, size = 32 }) {
  const color = COLORS[[...(name || '')].reduce((sum, c) => sum + c.charCodeAt(0), 0) % COLORS.length];
  return (
    <span
      className="avatar"
      title={name}
      style={{ width: size, height: size, fontSize: size * 0.4, background: color }}
    >
      {initials(name)}
    </span>
  );
}
