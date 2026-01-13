import axios, { AxiosInstance, AxiosError } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
      withCredentials: true, // For cookie-based auth
    });

    // Request interceptor
    this.client.interceptors.request.use(
      (config) => {
        // Add auth token if available
        const token = localStorage.getItem('authToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Handle unauthorized access
          localStorage.removeItem('authToken');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(email: string, password: string) {
    return this.client.post('/auth/login', { email, password });
  }

  async logout() {
    return this.client.post('/auth/logout');
  }

  async getCurrentUser() {
    return this.client.get('/Account/GetCurrentUser');
  }

  // Employee endpoints
  async getEmployees() {
    return this.client.get('/Employee/GetAllEmployees');
  }

  async getEmployee(id: string) {
    return this.client.get(`/Employee/GetEmployee/${id}`);
  }

  async createEmployee(data: any) {
    return this.client.post('/Employee/Create', data);
  }

  async updateEmployee(id: string, data: any) {
    return this.client.put(`/Employee/Update/${id}`, data);
  }

  async deleteEmployee(id: string) {
    return this.client.delete(`/Employee/Delete/${id}`);
  }

  // Attendance endpoints
  async getAttendanceReport(startDate: string, endDate: string) {
    return this.client.get('/Attendance/GetReport', {
      params: { startDate, endDate },
    });
  }

  async getEmployeeAttendance(employeeId: string, month: string, year: string) {
    return this.client.get(`/Attendance/GetEmployeeAttendance/${employeeId}`, {
      params: { month, year },
    });
  }

  // Leave endpoints
  async getLeaveApplications() {
    return this.client.get('/LeaveApplication/GetAll');
  }

  async submitLeaveApplication(data: any) {
    return this.client.post('/LeaveApplication/Submit', data);
  }

  async approveLeaveApplication(id: string) {
    return this.client.post(`/LeaveApproval/Approve/${id}`);
  }

  async rejectLeaveApplication(id: string, reason: string) {
    return this.client.post(`/LeaveApproval/Reject/${id}`, { reason });
  }

  async getLeaveBalance(employeeId: string) {
    return this.client.get(`/Leave/GetBalance/${employeeId}`);
  }

  // Department endpoints
  async getDepartments() {
    return this.client.get('/Department/GetAll');
  }

  // Salary endpoints
  async getSalaryDetails(employeeId: string) {
    return this.client.get(`/Salary/GetDetails/${employeeId}`);
  }

  // Dashboard endpoints
  async getDashboardStats() {
    return this.client.get('/Dashboard/GetStats');
  }

  async getDashboardChart(period: string) {
    return this.client.get('/Dashboard/GetChart', {
      params: { period },
    });
  }
}

export const apiClient = new ApiClient();
