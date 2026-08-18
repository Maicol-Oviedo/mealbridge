import { useEffect, useState } from 'react'
import { listDonations } from '../../api/donations'
import type {
  DonationFilters as DonationFilterValues,
  DonationLot,
} from '../../api/types'
import { CreateDonationForm } from './CreateDonationForm'
import { DonationDetail } from './DonationDetail'
import { DonationFilters } from './DonationFilters'
import { DonationList } from './DonationList'

const DashboardTitle = 'Lotes de donación'
const DashboardDescription =
  'Consulta y filtra los alimentos disponibles para rescate.'
const LoadingMessage = 'Cargando lotes de donación…'
const EmptyMessage = 'Todavía no hay lotes de donación.'
const FilteredEmptyMessage =
  'No hay lotes que coincidan con los filtros seleccionados.'
const UnexpectedErrorMessage =
  'No fue posible cargar los lotes de donación.'
const RetryLabel = 'Reintentar'
const CreatedSuccessMessage = 'El lote se publicó correctamente.'
const ClaimedSuccessMessage = 'El lote se reclamó correctamente.'
const PickedUpSuccessMessage =
  'El lote se marcó como recogido correctamente.'
const CancelledSuccessMessage = 'El lote se canceló correctamente.'

type DonationUpdateAction = 'claimed' | 'picked_up' | 'cancelled'

export function DonationDashboard() {
  const [filters, setFilters] = useState<DonationFilterValues>({})
  const [donations, setDonations] = useState<DonationLot[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reloadKey, setReloadKey] = useState(0)
  const [selectedDonationId, setSelectedDonationId] =
    useState<string | null>(null)
  const [successMessage, setSuccessMessage] =
    useState<string | null>(null)

  useEffect(() => {
    let isCurrentRequest = true

    setIsLoading(true)
    setError(null)

    listDonations(filters)
      .then((result) => {
        if (isCurrentRequest) {
          setDonations(result)
        }
      })
      .catch((requestError: unknown) => {
        if (isCurrentRequest) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : UnexpectedErrorMessage,
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
  }, [filters, reloadKey])

  const hasFilters = Boolean(filters.status || filters.foodCategory)

  function handleDonationCreated() {
    setSuccessMessage(CreatedSuccessMessage)
    setFilters({})
    setReloadKey((current) => current + 1)
  }

  function handleDonationUpdated(
    updatedDonation: DonationLot,
    action: DonationUpdateAction,
  ) {
    setDonations((current) =>
      current.map((donation) =>
        donation.id === updatedDonation.id
          ? updatedDonation
          : donation,
      ),
    )
    setSuccessMessage(
      action === 'claimed'
        ? ClaimedSuccessMessage
        : action === 'picked_up'
          ? PickedUpSuccessMessage
          : CancelledSuccessMessage,
    )
    setReloadKey((current) => current + 1)
  }

  return (
    <main className="donation-dashboard">
      <header className="dashboard-header">
        <h1>{DashboardTitle}</h1>
        <p>{DashboardDescription}</p>
      </header>

      {successMessage && (
        <p className="feedback feedback-success" role="status">
          {successMessage}
        </p>
      )}

      <div
        className={
          selectedDonationId
            ? 'dashboard-layout has-detail'
            : 'dashboard-layout'
        }
      >
        <div className="dashboard-content">
          <CreateDonationForm onCreated={handleDonationCreated} />
          <DonationFilters filters={filters} onChange={setFilters} />

          {isLoading && (
            <p className="state-message" aria-live="polite">
              {LoadingMessage}
            </p>
          )}

          {!isLoading && error && (
            <section className="feedback feedback-error" role="alert">
              <p>{error}</p>
              <button
                type="button"
                onClick={() => setReloadKey((current) => current + 1)}
              >
                {RetryLabel}
              </button>
            </section>
          )}

          {!isLoading && !error && donations.length === 0 && (
            <p className="state-message">
              {hasFilters ? FilteredEmptyMessage : EmptyMessage}
            </p>
          )}

          {!isLoading && !error && donations.length > 0 && (
            <DonationList
              donations={donations}
              onSelect={setSelectedDonationId}
            />
          )}
        </div>

        {selectedDonationId && (
          <DonationDetail
            donationId={selectedDonationId}
            onClose={() => setSelectedDonationId(null)}
            onUpdated={handleDonationUpdated}
          />
        )}
      </div>
    </main>
  )
}
