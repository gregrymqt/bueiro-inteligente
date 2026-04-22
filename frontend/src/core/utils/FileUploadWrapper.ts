import { AlertService } from '../alert/AlertService';

/**
 * Validates if a file is within the maximum allowed size.
 * Uses AlertService to notify the user if the file exceeds the limit.
 *
 * @param file The file to be validated.
 * @param maxSizeMB The maximum allowed size in Megabytes (default: 10MB).
 * @returns true if the file is valid, false if it exceeds the size limit.
 */
export const validateFileSize = (file: File, maxSizeMB: number = 10): boolean => {
  const maxSizeBytes = maxSizeMB * 1024 * 1024;

  if (file.size > maxSizeBytes) {
    AlertService.warning(
      'Arquivo muito grande',
      `O arquivo "${file.name}" excede o limite de ${maxSizeMB}MB permitidos.`
    );
    return false;
  }

  return true;
};
