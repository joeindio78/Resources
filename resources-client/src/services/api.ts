import axios from 'axios';
import {
  Resource,
  Competency,
  LoginRequest,
  LoginResponse,
} from '@/types/api';

const api = axios.create({
  baseURL: '/api/v1',
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const login = async (request: LoginRequest): Promise<LoginResponse> => {
  const response = await api.post<LoginResponse>('/login', request);
  localStorage.setItem('token', response.data.token);
  return response.data;
};

export const logout = () => {
  localStorage.removeItem('token');
};

interface ResourcesResponse {
  items: Resource[];
  totalPages: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface ResourcesQueryParams {
  name?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export const getResources = async (
  page: number,
  pageSize: number,
  params: ResourcesQueryParams = {}
): Promise<ResourcesResponse> => {
  const { data } = await api.get<ResourcesResponse>('/resources', {
    params: {
      page,
      pageSize,
      ...params,
    },
  });
  return data;
};

export const getResource = async (id: number): Promise<Resource> => {
  const { data } = await api.get<Resource>(`/resources/${id}`);
  return data;
};

export const createResource = async (resource: Omit<Resource, 'id'>): Promise<Resource> => {
  const { data } = await api.post<Resource>('/resources', resource);
  return data;
};

export const updateResource = async (
  id: number,
  resource: Omit<Resource, 'id'>
): Promise<Resource> => {
  const { data } = await api.put<Resource>(`/resources/${id}`, resource);
  return data;
};

export const deleteResource = async (id: number): Promise<void> => {
  await api.delete(`/resources/${id}`);
};

export const getCompetencies = async (): Promise<Competency[]> => {
  const response = await api.get<Competency[]>('/competencies');
  return response.data;
}; 