export class StringHelper {
  /**
   * Returns the string or null if the string was empty
   * @param str input string
   */
  static nullIfEmpty(str: string | null | undefined): string | null {
    return str == "" ? null : str ?? null;
  }
}


/**
 * Returns the string or null if the string was empty
 * @param str input string
 */
export function nullIfEmpty(str: string | null | undefined): string | null {
  return StringHelper.nullIfEmpty(str);
}
