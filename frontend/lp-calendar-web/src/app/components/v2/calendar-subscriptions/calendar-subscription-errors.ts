import { HttpErrorResponse } from '@angular/common/http';

/**
 * Returns a user-facing subscription error without distinguishing inaccessible subscriptions from missing ones.
 * @param error Error returned by a subscription API request.
 */
export function subscriptionErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 404) return 'This subscription was not found or is not accessible to your account.';
    if (error.status === 401) return 'Your session is not valid. Please log in and try again.';
    if (error.status === 400) return 'Check the settings and complete your user profile if you are logged in.';
  }
  return 'Please try again. If the problem persists, try again later.';
}
