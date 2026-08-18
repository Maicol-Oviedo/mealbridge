import {
  donationStatuses,
  foodCategories,
  type DonationFilters as DonationFilterValues,
  type DonationStatus,
  type FoodCategory,
} from '../../api/types'

const StatusLabel = 'Estado'
const FoodCategoryLabel = 'Categoría'
const AllStatusesLabel = 'Todos los estados'
const AllCategoriesLabel = 'Todas las categorías'
const FiltersTitle = 'Filtrar lotes'
const FiltersDescription =
  'Refina los resultados por estado y categoría.'
const FiltersTitleId = 'donation-filters-title'

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

interface DonationFiltersProps {
  filters: DonationFilterValues
  onChange: (filters: DonationFilterValues) => void
}

export function DonationFilters({
  filters,
  onChange,
}: DonationFiltersProps) {
  return (
    <section
      className="panel filters-panel"
      aria-labelledby={FiltersTitleId}
    >
      <header className="section-heading filters-heading">
        <h2 id={FiltersTitleId}>{FiltersTitle}</h2>
        <p>{FiltersDescription}</p>
      </header>

      <label>
        {StatusLabel}
        <select
          value={filters.status ?? ''}
          onChange={(event) =>
            onChange({
              ...filters,
              status: event.target.value
                ? event.target.value as DonationStatus
                : undefined,
            })
          }
        >
          <option value="">{AllStatusesLabel}</option>
          {donationStatuses.map((status) => (
            <option key={status} value={status}>
              {statusLabels[status]}
            </option>
          ))}
        </select>
      </label>

      <label>
        {FoodCategoryLabel}
        <select
          value={filters.foodCategory ?? ''}
          onChange={(event) =>
            onChange({
              ...filters,
              foodCategory: event.target.value
                ? event.target.value as FoodCategory
                : undefined,
            })
          }
        >
          <option value="">{AllCategoriesLabel}</option>
          {foodCategories.map((category) => (
            <option key={category} value={category}>
              {foodCategoryLabels[category]}
            </option>
          ))}
        </select>
      </label>
    </section>
  )
}
