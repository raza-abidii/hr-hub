import React, { createContext, useContext, useState, useCallback } from 'react';

export type UserRole = 'admin' | 'employee';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  department?: string;
  designation?: string;
  avatar?: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Mock users for demo
const MOCK_USERS: Record<string, { password: string; user: User }> = {
  'admin@sfon.com': {
    password: 'admin123',
    user: {
      id: '1',
      name: 'John Admin',
      email: 'admin@sfon.com',
      role: 'admin',
      department: 'Human Resources',
      designation: 'HR Manager',
    },
  },
  'emp@sfon.com': {
    password: 'emp123',
    user: {
      id: '2',
      name: 'Sarah Employee',
      email: 'emp@sfon.com',
      role: 'employee',
      department: 'Information Technology',
      designation: 'Software Developer',
    },
  },
};

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  const login = useCallback(async (email: string, password: string): Promise<boolean> => {
    // Simulate API call delay
    await new Promise((resolve) => setTimeout(resolve, 800));

    const mockUser = MOCK_USERS[email.toLowerCase()];
    if (mockUser && mockUser.password === password) {
      setUser(mockUser.user);
      return true;
    }
    return false;
  }, []);

  const logout = useCallback(() => {
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
