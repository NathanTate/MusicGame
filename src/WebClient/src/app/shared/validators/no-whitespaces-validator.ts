import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function noWhiteSpacesValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if ((control.value as string || '').trim().length === 0) {
      return { whitespaces: true }
    }
    return null;
  }
}