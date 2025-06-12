export interface Resource {
  id: number;
  name: string;
  birthDate?: string;
  yearsOfExperience: number;
  competencies: Competency[];
}

export interface Competency {
  id: number;
  name: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface CreateResourceRequest {
  name: string;
  birthDate?: string;
  yearsOfExperience: number;
  competencyIds: number[];
}

export interface UpdateResourceRequest {
  name?: string;
  birthDate?: string;
  yearsOfExperience?: number;
  competencyIds: number[];
} 