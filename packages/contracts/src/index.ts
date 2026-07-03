export interface ApiResponse<T> {
  success: boolean;
  data: T;
  errors: string[];
  correlationId: string;
}

export interface HealthStatus {
  status: string;
  timestamp: string;
  service: string;
  version: string;
}

export interface WorkflowDto {
  id: string;
  userId: string;
  status: string;
  currentStep: string | null;
  currentUrl: string | null;
  createdAt: string;
}

export interface ProviderConfigDto {
  id: string;
  providerType: string;
  modelName: string;
  baseUrl: string | null;
  temperature: number;
  maxTokens: number;
  isDefault: boolean;
}

export interface DocumentDto {
  id: string;
  documentType: string;
  displayName: string;
  mimeType: string;
  fileSize: number;
  createdAt: string;
}
