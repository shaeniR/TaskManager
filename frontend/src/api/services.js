import api from './axios';

export const authApi = {
  login: (data) => api.post('/auth/login', data).then((r) => r.data),
  register: (data) => api.post('/auth/register', data).then((r) => r.data),
  me: () => api.get('/auth/me').then((r) => r.data),
};

export const projectApi = {
  getAll: () => api.get('/projects').then((r) => r.data),
  get: (id) => api.get(`/projects/${id}`).then((r) => r.data),
  create: (data) => api.post('/projects', data).then((r) => r.data),
  update: (id, data) => api.put(`/projects/${id}`, data).then((r) => r.data),
  remove: (id) => api.delete(`/projects/${id}`),
  addMember: (id, userId) => api.post(`/projects/${id}/members`, { userId }).then((r) => r.data),
  removeMember: (id, userId) => api.delete(`/projects/${id}/members/${userId}`),
  getTasks: (id, params) => api.get(`/projects/${id}/tasks`, { params }).then((r) => r.data),
};

export const taskApi = {
  create: (data) => api.post('/tasks', data).then((r) => r.data),
  update: (id, data) => api.put(`/tasks/${id}`, data).then((r) => r.data),
  updateStatus: (id, status) => api.patch(`/tasks/${id}/status`, { status }).then((r) => r.data),
  remove: (id) => api.delete(`/tasks/${id}`),
  getComments: (id) => api.get(`/tasks/${id}/comments`).then((r) => r.data),
  addComment: (id, content) => api.post(`/tasks/${id}/comments`, { content }).then((r) => r.data),
};

export const dashboardApi = {
  get: () => api.get('/dashboard').then((r) => r.data),
};

export const userApi = {
  search: (search) => api.get('/users', { params: { search } }).then((r) => r.data),
};
