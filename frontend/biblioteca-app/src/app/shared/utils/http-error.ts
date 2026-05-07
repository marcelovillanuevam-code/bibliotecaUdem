import { HttpErrorResponse } from '@angular/common/http';

const SERVER_ERROR_MESSAGE = 'Ocurrió un error en el servidor. Intenta de nuevo.';

export function resolveHttpError(error: unknown, fallbackMessage: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallbackMessage;
  }

  if (error.status >= 500) {
    return SERVER_ERROR_MESSAGE;
  }

  const body = error.error as Record<string, unknown> | null | undefined;

  if (body && typeof body === 'object') {
    const detail = typeof body['detail'] === 'string' ? body['detail'] : undefined;
    const message = typeof body['message'] === 'string' ? body['message'] : undefined;
    const errorField = typeof body['error'] === 'string' ? body['error'] : undefined;
    const title = typeof body['title'] === 'string' ? body['title'] : undefined;

    return detail ?? message ?? errorField ?? title ?? fallbackMessage;
  }

  return fallbackMessage;
}
