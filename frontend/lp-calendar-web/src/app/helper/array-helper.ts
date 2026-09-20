class ArrayHelper {
  static makeRealArray<T>(arr: (T | T[] | undefined)): T[] {
    return arr == null ? [] : Array.isArray(arr) ? arr : [arr];
  }
}


export function makeRealArray<T>(arr: (T | T[] | undefined)): T[] {
  return ArrayHelper.makeRealArray(arr);
}
