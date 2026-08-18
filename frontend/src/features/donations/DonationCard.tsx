import type {
  DonationLot,
  DonationStatus,
  DonationUnit,
  FoodCategory,
} from '../../api/types'

const PickupLabel = 'Recogida'
const AvailabilityLabel = 'Disponible'
const ClaimedByLabel = 'Reclamado por'
const ViewDetailLabel = 'Ver detalle'

const statusLabels: Record<DonationStatus, string> = {
  available: 'Disponible',
  claimed: 'Reclamado',
  picked_up: 'Recogido',
  cancelled: 'Cancelado',
  expired: 'Expirado',
}

const foodCategoryLabels: Record<FoodCategory, string> = {
  bakery: 'Panadería',
  produce: 'Frutas y verduras',
  dairy: 'Lácteos',
  prepared: 'Comida preparada',
  other: 'Otros',
}

const unitLabels: Record<DonationUnit, string> = {
  portions: 'porciones',
  kg: 'kg',
  loaves: 'panes',
  boxes: 'cajas',
}

const dateFormatter = new Intl.DateTimeFormat('es-CO', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

interface DonationCardProps {
  donation: DonationLot
  onSelect?: (id: string) => void
}

export function DonationCard({
  donation,
  onSelect,
}: DonationCardProps) {
  return (
    <article className="donation-card">
      <header className="donation-card-header">
        <p className="business-name">{donation.businessName}</p>
        <span className={`status-badge status-${donation.status}`}>
          {statusLabels[donation.status]}
        </span>
      </header>

      <h3 className="donation-title">{donation.title}</h3>
      {donation.description && (
        <p className="donation-description">{donation.description}</p>
      )}

      <dl className="donation-facts">
        <div className="fact-item">
          <dt>{foodCategoryLabels[donation.foodCategory]}</dt>
          <dd>
            {donation.quantity} {unitLabels[donation.unit]}
          </dd>
        </div>
        <div className="fact-item fact-wide">
          <dt>{PickupLabel}</dt>
          <dd>{donation.pickupAddress}</dd>
        </div>
        <div className="fact-item fact-wide">
          <dt>{AvailabilityLabel}</dt>
          <dd>
            <time dateTime={donation.availableFrom}>
              {dateFormatter.format(new Date(donation.availableFrom))}
            </time>
            {' – '}
            <time dateTime={donation.availableUntil}>
              {dateFormatter.format(new Date(donation.availableUntil))}
            </time>
          </dd>
        </div>
        {donation.claimedBy && (
          <div className="fact-item fact-wide claimed-fact">
            <dt>{ClaimedByLabel}</dt>
            <dd>{donation.claimedBy}</dd>
          </div>
        )}
      </dl>

      {onSelect && (
        <button
          className="secondary-button"
          type="button"
          onClick={() => onSelect(donation.id)}
        >
          {ViewDetailLabel}
        </button>
      )}
    </article>
  )
}
