import * as which from 'which';
import { isErrnoException } from './errors';

/**
 * Find the specified executable (if it exists) on the current environment's `PATH`.
 * 
 * @param executableName The executable name (with or without extension).
 * @returns {Promise<string | null>} A promise that resolves to the full path, or `null` if the executable was not found.
 */
export function find(executableName: string): Promise<string | null> {
    return new Promise<string | null>((resolve, reject) => {
        which(executableName, (error, resolvedPath) => {
            if (error) {
                if (isErrnoException(error) && error.code === 'ENOENT')
                    resolve(null);
                else
                    reject(error);
            }
            else
                resolve(resolvedPath);
        });
    });
}
