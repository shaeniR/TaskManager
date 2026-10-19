import Avatar from './Avatar';
import { formatDate, isOverdue } from '../utils/format';

export default function TaskCard({ task, onClick, onDragStart }) {
  const overdue = isOverdue(task);

  return (
    <div
      className="task-card"
      draggable
      onDragStart={(e) => onDragStart(e, task)}
      onClick={() => onClick(task)}
    >
      <div className="task-card-top">
        <span className={`badge priority-${task.priority.toLowerCase()}`}>{task.priority}</span>
        {task.commentCount > 0 && <span className="muted small">💬 {task.commentCount}</span>}
      </div>
      <h4>{task.title}</h4>
      {task.description && <p className="task-desc">{task.description}</p>}
      <div className="task-card-bottom">
        <span className={`small ${overdue ? 'text-danger' : 'muted'}`}>
          📅 {formatDate(task.dueDate)} {overdue && '(overdue)'}
        </span>
        {task.assignedTo ? <Avatar name={task.assignedTo.name} size={26} /> : <span className="muted small">Unassigned</span>}
      </div>
    </div>
  );
}
