export const foodCategories = [
  'bakery',
  'produce',
  'dairy',
  'prepared',
  'other',
] as const

export const donationUnits = [
  'portions',
  'kg',
  'loaves',
  'boxes',
] as const

export const donationStatuses = [
  'available',
  'claimed',
  'picked_up',
  'cancelled',
  'expired',
] as const

export type FoodCategory = (typeof foodCategories)[number]
export type DonationUnit = (typeof donationUnits)[number]
export type DonationStatus = (typeof donationStatuses)[number]

export interface DonationLot {
  id: string
  businessName: string
  title: string
  description: string | null
  foodCategory: FoodCategory
  quantity: number
  unit: DonationUnit
  pickupAddress: string
  availableFrom: string
  availableUntil: string
  status: DonationStatus
  claimedBy: string | null
  claimedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateDonationRequest {
  businessName: string
  title: string
  description?: string | null
  foodCategory: FoodCategory
  quantity: number
  unit: DonationUnit
  pickupAddress: string
  availableFrom: string
  availableUntil: string
}

export interface DonationFilters {
  status?: DonationStatus
  foodCategory?: FoodCategory
}

export interface ClaimDonationRequest {
  coordinatorName: string
}

export interface ChangeDonationStatusRequest {
  status: DonationStatus
}

export interface ApiResponseEnvelope<T> {
  succeeded: boolean
  data: T | null
  error: string | null
}
