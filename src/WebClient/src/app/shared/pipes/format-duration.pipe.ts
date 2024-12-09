import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatDuration',
  standalone: true
})
export class FormatDurationPipe implements PipeTransform {

  transform(duration: number): unknown {
    const dateObj = new Date(duration * 1000);
    const hours = dateObj.getUTCHours();
    const minutes = dateObj.getUTCMinutes();
    const seconds = dateObj.getSeconds();
    const formattedTime = (hours ? hours.toString().padStart(2, '0') + ':' : '') +
      minutes.toString().padStart(2, '0') + ':' +
      seconds.toString().padStart(2, '0');

    return formattedTime;
  }

}
