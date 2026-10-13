export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  details?: string | null;
}