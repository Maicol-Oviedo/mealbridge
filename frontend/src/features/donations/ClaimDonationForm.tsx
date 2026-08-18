import { useState, type FormEvent } from 'react'
import { ApiError, claimDonation } from '../../api/donations'
import type { DonationLot } from '../../api/types'
import {
  applySpanishValidationMessage,
  clearSpanishValidationMessage,
} from './formValidation'

const CoordinatorNameLabel = 'Nombre del coordinador'
const ClaimLabel = 'Reclamar lote'
const ClaimingLabel = 'Reclamando…'
const UnexpectedErrorMessage = 'No fue posible reclamar el lote.'

interface ClaimDonationFormProps {
  donationId: string
  onClaimed: (donation: DonationLot) => void
}

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError && error.status > 0) {
    return `${error.message} (HTTP ${error.status})`
  }

  return error instanceof Error
    ? error.message
    : UnexpectedErrorMessage
}

export function ClaimDonationForm({
  donationId,
  onClaimed,
}: ClaimDonationFormProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    const coordinatorName = String(
      data.get('coordinatorName') ?? '',
    ).trim()

    setIsSubmitting(true)
    setError(null)

    try {
      const donation = await claimDonation(donationId, {
        coordinatorName,
      })
      form.reset()
      onClaimed(donation)
    } catch (requestError: unknown) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="claim-form" onSubmit={handleSubmit}>
      <label>
        {CoordinatorNameLabel}
        <input
          name="coordinatorName"
          type="text"
          maxLength={120}
          onInvalid={applySpanishValidationMessage}
          onInput={clearSpanishValidationMessage}
          required
        />
      </label>

      {error && (
        <p className="feedback feedback-error" role="alert">
          {error}
        </p>
      )}

      <button
        className="primary-button"
        type="submit"
        disabled={isSubmitting}
      >
        {isSubmitting ? ClaimingLabel : ClaimLabel}
      </button>
    </form>
  )
}
