export const toFormData = (jsonObject: any, parentKey?: string, carryFormData?: FormData,) => {
  const formData = carryFormData || new FormData();
  let index = 0;

  for (var key in jsonObject) {
    if (jsonObject.hasOwnProperty(key)) {
      if (jsonObject[key] !== null && jsonObject[key] !== undefined) {
        var propName = parentKey || key;
        if (parentKey && isObject(jsonObject)) {
          propName = parentKey + '[' + key + ']';
        }
        if (parentKey && isArray(jsonObject)) {
          propName = parentKey + '[' + index + ']';
        }
        if (jsonObject[key] instanceof File) {
          formData.append(propName, jsonObject[key]);
        } else if (jsonObject[key] instanceof FileList) {
          for (var j = 0; j < jsonObject[key].length; j++) {
            formData.append(propName + '[' + j + ']', jsonObject[key].item(j));
          }
        } else if (jsonObject[key] instanceof Date) {
          formData.append(propName, jsonObject[key].toISOString());
        } else if (isArray(jsonObject[key]) || isObject(jsonObject[key])) {
          toFormData(jsonObject[key], propName, formData);
        } else if (typeof jsonObject[key] === 'boolean') {
          formData.append(propName, jsonObject[key]);
        } else {
          formData.append(propName, jsonObject[key]);
        }
      }
    }
    index++;
  }
  return formData;
}

function isArray(val: unknown) {
  const toString = ({}).toString;
  return toString.call(val) === '[object Array]';
}

function isObject(val: unknown) {
  return !isArray(val) && typeof val === 'object' && !!val;
}