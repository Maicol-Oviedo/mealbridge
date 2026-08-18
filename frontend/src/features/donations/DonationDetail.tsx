import { useEffect, useState } from 'react'
import {
  ApiError,
  changeDonationStatus,
  getDonation,
} from '../../api/donations'
import type { DonationLot, DonationStatus } from '../../api/types'
import { ClaimDonationForm } from './ClaimDonationForm'
import { DonationCard } from './DonationCard'

const DetailTitle = 'Detalle del lote'
const LoadingMessage = 'Cargando el detalle…'
const CloseLabel = 'Cerrar detalle'
const PickedUpLabel = 'Marcar como recogido'
const CancelLabel = 'Cancelar lote'
const UpdatingLabel = 'Actualizando…'
const UnexpectedLoadErrorMessage =
  'No fue posible cargar el detalle del lote.'
const UnexpectedUpdateErrorMessage =
  'No fue posible actualizar el estado del lote.'

interface DonationDetailProps {
  donationId: string
  onClose: () => void
  onUpdated: (
    donation: DonationLot,
    action: 'claimed' | 'picked_up' | 'cancelled',
  ) => void
}

function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiError && error.status > 0) {
    return `${error.message} (HTTP ${error.status})`
  }

  return error instanceof Error ? error.message : fallback
}

export function DonationDetail({
  donationId,
  onClose,
  onUpdated,
}: DonationDetailProps) {
  const [donation, setDonation] = useState<DonationLot | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isUpdating, setIsUpdating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let isCurrentRequest = true

    setDonation(null)
    setIsLoading(true)
    setError(null)

    getDonation(donationId)
      .then((result) => {
        if (isCurrentRequest) {
          setDonation(result)
        }
      })
      .catch((requestError: unknown) => {
        if (isCurrentRequest) {
          setError(
            getErrorMessage(
              requestError,
              UnexpectedLoadErrorMessage,
            ),
          )
        }
      })
      .finally(() => {
        if (isCurrentRequest) {
          setIsLoading(false)
        }
      })

    return () => {
      isCurrentRequest = false
    }
  }, [donationId])

  function handleUpdated(
    updatedDonation: DonationLot,
    action: 'claimed' | 'picked_up' | 'cancelled',
  ) {
    setDonation(updatedDonation)
    setError(null)
    onUpdated(updatedDonation, action)
  }

  function handleClaimed(updatedDonation: DonationLot) {
    handleUpdated(updatedDonation, 'claimed')
  }

  async function updateStatus(
    status: Extract<DonationStatus, 'picked_up' | 'cancelled'>,
  ) {
    setIsUpdating(true)
    setError(null)

    try {
      const updatedDonation = await changeDonationStatus(
        donationId,
        { status },
      )
      handleUpdated(updatedDonation, status)
    } catch (requestError: unknown) {
      setError(
        getErrorMessage(
          requestError,
          UnexpectedUpdateErrorMessage,
        ),
      )
    } finally {
      setIsUpdating(false)
    }
  }

  return (
    <aside className="detail-panel" aria-label={DetailTitle}>
      <header className="detail-header">
        <h2>{DetailTitle}</h2>
        <button
          className="secondary-button"
          type="button"
          onClick={onClose}
        >
          {CloseLabel}
        </button>
      </header>

      {isLoading && <p aria-live="polite">{LoadingMessage}</p>}
      {error && (
        <p className="feedback feedback-error" role="alert">
          {error}
        </p>
      )}

      {!isLoading && donation && (
        <>
          <DonationCard donation={donation} />

          {donation.status === 'available' && (
            <ClaimDonationForm
              donationId={donation.id}
              onClaimed={handleClaimed}
            />
          )}

          {donation.status === 'claimed' && (
            <div className="detail-actions">
              <button
                className="primary-button"
                type="button"
                disabled={isUpdating}
                onClick={() => updateStatus('picked_up')}
              >
                {isUpdating ? UpdatingLabel : PickedUpLabel}
              </button>
              <button
                className="danger-button"
                type="button"
                disabled={isUpdating}
                onClick={() => updateStatus('cancelled')}
              >
                {isUpdating ? UpdatingLabel : CancelLabel}
              </button>
            </div>
          )}
        </>
      )}
    </aside>
  )
}
