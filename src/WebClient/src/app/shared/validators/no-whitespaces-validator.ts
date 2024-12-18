import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function noWhiteSpacesValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string || '';
    if (value && value.length > 0 && value.trim().length === 0) {
      return { whitespaces: true }
    }
    return null;
  }
}