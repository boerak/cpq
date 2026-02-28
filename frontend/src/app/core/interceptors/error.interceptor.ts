import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError, TimeoutError } from 'rxjs';
import { timeout } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    timeout(30000),
    catchError((error: unknown) => {
      if (error instanceof TimeoutError) {
        snackBar.open(
          'Verzoek verlopen. Probeer het opnieuw.',
          'Sluiten',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
        return throwError(() => new Error('Request timeout'));
      }

      if (error instanceof HttpErrorResponse) {
        if (error.status === 0) {
          // Network error
          snackBar.open(
            'Netwerkfout. Controleer uw verbinding.',
            'Sluiten',
            { duration: 5000, panelClass: ['error-snackbar'] }
          );
        } else if (error.status === 409) {
          // Version conflict
          snackBar.open(
            'Versieconflict gedetecteerd. Pagina wordt herladen.',
            'Sluiten',
            { duration: 5000, panelClass: ['warning-snackbar'] }
          );
          // Reload the page after a brief delay
          setTimeout(() => window.location.reload(), 2000);
        } else if (error.status === 502 || error.status === 503) {
          // Rules engine unavailable
          snackBar.open(
            'Regelmotor niet beschikbaar. Probeer het later opnieuw.',
            'Sluiten',
            { duration: 8000, panelClass: ['error-snackbar'] }
          );
        } else if (error.status === 500) {
          snackBar.open(
            'Serverfout. Probeer het opnieuw.',
            'Sluiten',
            { duration: 5000, panelClass: ['error-snackbar'] }
          );
        }
      }

      return throwError(() => error);
    })
  );
};
