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

/**
 * Replaces MediaWiki links with their displayed text, or their target when no displayed text is set.
 */
export function replaceMediaWikiLinksInString<T extends string | null | undefined>(input: T): T extends string ? string : T;
export function replaceMediaWikiLinksInString(input: string | null | undefined): string | null | undefined {
  if (!input) {
    return input;
  }

  return input.replace(/\[\[(?:.*?\|)?(.*?)]]/g, "$1");
}
