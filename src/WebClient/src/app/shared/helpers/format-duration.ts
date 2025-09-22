import { isNullOrUndefined } from 'is-what';

export const formatDuration = (duration: number | null | undefined): string => {
  if (isNullOrUndefined(duration)) {
    return '-:--'
  }

  const dateObj = new Date(duration);
  const hours = dateObj.getUTCHours();
  const minutes = dateObj.getUTCMinutes();
  const seconds = dateObj.getSeconds();
  const formattedTime = (hours ? hours.toString().padStart(2, '0') + ':' : '') +
    minutes.toString().padStart(2, '0') + ':' +
    seconds.toString().padStart(2, '0');

  return formattedTime;
}