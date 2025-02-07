import { HttpParams } from "@angular/common/http"

export const generateHttpParams = <T extends Object>(params: T) => {
  let httpParams = new HttpParams();

  for (let key in params) {
    if (Object.hasOwn(params, key)) {
      const value = params[key];

      if (value === null || value === undefined) {
        continue;
      }

      if (Array.isArray(value)) {
        value.forEach((v: number | string | boolean) => {
          httpParams = httpParams.append(key, v);
        })
      } else {
        httpParams = httpParams.append(key, String(value))
      }
    }
  }
  return httpParams;
}