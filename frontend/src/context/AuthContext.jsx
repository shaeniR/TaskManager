import { createContext, useContext, useState } from 'react';
import { authApi } from '../api/services';
import { TOKEN_KEY, USER_KEY } from '../api/axios';

const AuthContext = createContext(null);

function loadUser() {
  try {
    return JSON.parse(localStorage.getItem(USER_KEY));
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadUser);

  const saveSession = ({ token, user }) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    setUser(user);
  };

  const login = async (email, password) => saveSession(await authApi.login({ email, password }));

  const register = async (name, email, password) =>
    saveSession(await authApi.register({ name, email, password }));

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setUser(null);
  };

  const value = { user, isAdmin: user?.role === 'Admin', login, register, logout };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
