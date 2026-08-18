import type { FormEvent, InvalidEvent } from 'react'

type ValidatableControl =
  | HTMLInputElement
  | HTMLSelectElement
  | HTMLTextAreaElement

const RequiredFieldMessage = 'Completa este campo.'
const MinimumValueMessage = 'El valor debe ser igual o mayor que 1.'
const InvalidNumberMessage = 'Ingresa un número válido.'
const InvalidDateMessage = 'Ingresa una fecha y hora válidas.'
const InvalidValueMessage = 'Ingresa un valor válido.'

export function applySpanishValidationMessage(
  event: InvalidEvent<ValidatableControl>,
) {
  const control = event.currentTarget
  const validity = control.validity

  if (validity.valueMissing) {
    control.setCustomValidity(RequiredFieldMessage)
    return
  }

  if (validity.rangeUnderflow) {
    control.setCustomValidity(MinimumValueMessage)
    return
  }

  if (validity.badInput && control instanceof HTMLInputElement) {
    control.setCustomValidity(
      control.type === 'datetime-local'
        ? InvalidDateMessage
        : InvalidNumberMessage,
    )
    return
  }

  control.setCustomValidity(InvalidValueMessage)
}

export function clearSpanishValidationMessage(
  event: FormEvent<ValidatableControl>,
) {
  event.currentTarget.setCustomValidity('')
}
