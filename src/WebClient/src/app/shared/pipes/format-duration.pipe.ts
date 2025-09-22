import { Pipe, PipeTransform } from '@angular/core';
import { formatDuration } from '../helpers/format-duration';

@Pipe({
  name: 'formatDuration',
  standalone: true
})
export class FormatDurationPipe implements PipeTransform {

  transform(duration: number): unknown {
    return formatDuration(duration);
  }

}
