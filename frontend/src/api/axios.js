import axios from 'axios';

export const TOKEN_KEY = 'tm_token';
export const USER_KEY = 'tm_user';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
});

// Attach the JWT to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// If the token expired / is invalid, log the user out
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthCall = error.config?.url?.startsWith('/auth/');
    if (error.response?.status === 401 && !isAuthCall) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Turns any API error into a readable message (custom errors + FluentValidation errors)
export function getErrorMessage(error) {
  const data = error.response?.data;
  if (data?.errors) return Object.values(data.errors).flat().join(' ');
  if (data?.message) return data.message;
  if (!error.response) return 'Cannot reach the server. Is the API running?';
  return 'Something went wrong.';
}

export default api;
