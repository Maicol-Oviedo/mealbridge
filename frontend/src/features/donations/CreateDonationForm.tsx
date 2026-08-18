import type { FormEvent } from 'react'
import { createDonation } from '../../api/donations'
import {
  donationUnits,
  foodCategories,
  type DonationUnit,
  type FoodCategory,
} from '../../api/types'
import { useState } from 'react'
import {
  applySpanishValidationMessage,
  clearSpanishValidationMessage,
} from './formValidation'

const FormTitle = 'Publicar un lote'
const FormDescription =
  'Completa la información para coordinar una recogida segura.'
const BusinessNameLabel = 'Nombre del negocio'
const TitleLabel = 'Título'
const DescriptionLabel = 'Descripción'
const FoodCategoryLabel = 'Categoría'
const QuantityLabel = 'Cantidad'
const UnitLabel = 'Unidad'
const PickupAddressLabel = 'Dirección de recogida'
const AvailableFromLabel = 'Disponible desde'
const AvailableUntilLabel = 'Disponible hasta'
const SubmitLabel = 'Publicar lote'
const SubmittingLabel = 'Publicando…'
const InvalidAvailabilityMessage =
  'La fecha final debe ser posterior a la fecha inicial.'
const UnexpectedErrorMessage =
  'No fue posible publicar el lote de donación.'

const foodCategoryLabels: Record<FoodCategory, string> = {
  bakery: 'Panadería',
  produce: 'Frutas y verduras',
  dairy: 'Lácteos',
  prepared: 'Comida preparada',
  other: 'Otros',
}

const unitLabels: Record<DonationUnit, string> = {
  portions: 'Porciones',
  kg: 'Kilogramos',
  loaves: 'Panes',
  boxes: 'Cajas',
}

interface CreateDonationFormProps {
  onCreated: () => void
}

function getRequiredValue(data: FormData, name: string): string {
  return String(data.get(name) ?? '').trim()
}

export function CreateDonationForm({
  onCreated,
}: CreateDonationFormProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    const availableFrom = getRequiredValue(data, 'availableFrom')
    const availableUntil = getRequiredValue(data, 'availableUntil')

    if (new Date(availableUntil) <= new Date(availableFrom)) {
      setError(InvalidAvailabilityMessage)
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      await createDonation({
        businessName: getRequiredValue(data, 'businessName'),
        title: getRequiredValue(data, 'title'),
        description: getRequiredValue(data, 'description') || null,
        foodCategory: getRequiredValue(data, 'foodCategory') as FoodCategory,
        quantity: Number(getRequiredValue(data, 'quantity')),
        unit: getRequiredValue(data, 'unit') as DonationUnit,
        pickupAddress: getRequiredValue(data, 'pickupAddress'),
        availableFrom: new Date(availableFrom).toISOString(),
        availableUntil: new Date(availableUntil).toISOString(),
      })
      form.reset()
      onCreated()
    } catch (requestError: unknown) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : UnexpectedErrorMessage,
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="panel create-form-panel">
      <header className="form-section-header">
        <div>
          <h2>{FormTitle}</h2>
          <p>{FormDescription}</p>
        </div>
      </header>

      <form className="form-grid" onSubmit={handleSubmit}>
        <label className="floating-field field-half">
          <input
            name="businessName"
            type="text"
            placeholder=" "
            maxLength={120}
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{BusinessNameLabel}</span>
        </label>

        <label className="floating-field field-half">
          <input
            name="title"
            type="text"
            placeholder=" "
            maxLength={80}
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{TitleLabel}</span>
        </label>

        <label className="floating-field field-full">
          <textarea
            name="description"
            placeholder=" "
            maxLength={500}
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
          />
          <span className="floating-label">{DescriptionLabel}</span>
        </label>

        <label className="floating-field field-third persistent-label">
          <select
            name="foodCategory"
            defaultValue="bakery"
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          >
            {foodCategories.map((category) => (
              <option key={category} value={category}>
                {foodCategoryLabels[category]}
              </option>
            ))}
          </select>
          <span className="floating-label">{FoodCategoryLabel}</span>
        </label>

        <label className="floating-field field-third">
          <input
            name="quantity"
            type="number"
            placeholder=" "
            min={1}
            step={1}
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{QuantityLabel}</span>
        </label>

        <label className="floating-field field-third persistent-label">
          <select
            name="unit"
            defaultValue="portions"
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          >
            {donationUnits.map((unit) => (
              <option key={unit} value={unit}>
                {unitLabels[unit]}
              </option>
            ))}
          </select>
          <span className="floating-label">{UnitLabel}</span>
        </label>

        <label className="floating-field field-full">
          <input
            name="pickupAddress"
            type="text"
            placeholder=" "
            maxLength={200}
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{PickupAddressLabel}</span>
        </label>

        <label className="floating-field field-half persistent-label">
          <input
            name="availableFrom"
            type="datetime-local"
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{AvailableFromLabel}</span>
        </label>

        <label className="floating-field field-half persistent-label">
          <input
            name="availableUntil"
            type="datetime-local"
            onInvalid={applySpanishValidationMessage}
            onInput={clearSpanishValidationMessage}
            required
          />
          <span className="floating-label">{AvailableUntilLabel}</span>
        </label>

        {error && (
          <p className="feedback feedback-error form-wide" role="alert">
            {error}
          </p>
        )}

        <button
          className="primary-button form-wide"
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting ? SubmittingLabel : SubmitLabel}
        </button>
      </form>
    </section>
  )
}
