import { isAxiosError } from 'axios';

interface ValidationProblemDetails {
  errors: Record<string, string[]>;
}

function isValidationProblemDetails(data: unknown): data is ValidationProblemDetails {
  return typeof data === 'object' && data !== null && 'errors' in data;
}

// The API returns two different 400 shapes: FluentValidation failures come through the global
// exception handler as RFC 9457 ValidationProblemDetails ({ errors: { Field: ["msg"] } }),
// while Result.Failure(["msg"]) business-rule failures are serialized as a plain string array.
export function extractErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!isAxiosError(error)) {
    return fallback;
  }

  const data: unknown = error.response?.data;

  if (Array.isArray(data)) {
    return data.join(' ');
  }

  if (isValidationProblemDetails(data)) {
    return Object.values(data.errors).flat().join(' ');
  }

  if (typeof data === 'object' && data !== null && 'title' in data) {
    return String((data as { title: unknown }).title);
  }

  return fallback;
}
