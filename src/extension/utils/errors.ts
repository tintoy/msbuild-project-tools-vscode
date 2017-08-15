/**
 * Determine whether the specified Error is a NodeJS.ErrnoException.
 * 
 * @param error The error to examine.
 * @returns true, if the error is a NodeJS.ErrnoException; otherwise, false.
 */
export function isErrnoException(error: Error): error is NodeJS.ErrnoException {
    return typeof error === 'object' && typeof error['code'] !== 'undefined';
}
