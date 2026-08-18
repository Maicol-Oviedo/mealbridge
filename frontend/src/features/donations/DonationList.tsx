import type { DonationLot } from '../../api/types'
import { DonationCard } from './DonationCard'

const ListTitle = 'Lotes publicados'
const ListDescription =
  'Consulta los lotes que coinciden con los filtros seleccionados.'
const ListTitleId = 'donation-list-title'

interface DonationListProps {
  donations: DonationLot[]
  onSelect: (id: string) => void
}

export function DonationList({
  donations,
  onSelect,
}: DonationListProps) {
  return (
    <section className="donations-section" aria-labelledby={ListTitleId}>
      <header className="section-heading list-heading">
        <h2 id={ListTitleId}>{ListTitle}</h2>
        <p>{ListDescription}</p>
      </header>

      <div className="donation-list">
        {donations.map((donation) => (
          <DonationCard
            key={donation.id}
            donation={donation}
            onSelect={onSelect}
          />
        ))}
      </div>
    </section>
  )
}
