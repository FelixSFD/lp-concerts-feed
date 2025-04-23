import { Pipe, PipeTransform } from '@angular/core';
@Pipe({
  name: 'timeSpan'
})
export class TimeSpanPipe implements PipeTransform {
  transform(value: number): string {
    let h = Math.floor(value / 60);
    let m = Math.floor(value % 60);

    let txt = "";
    if (h == 1) {
      txt += h + " hour";
    } else if (h > 1) {
      txt += h + " hours";
    }

    if (m > 0) {
      txt += " " + m + " ";
    }
    if (m == 1) {
      txt += "minutes";
    } else if (m > 1) {
      txt += "minutes";
    }

    return txt;
  }
}
