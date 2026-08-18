import type {
  ApiResponseEnvelope,
  ChangeDonationStatusRequest,
  ClaimDonationRequest,
  CreateDonationRequest,
  DonationFilters,
  DonationLot,
} from './types'

const ApiUrlRequiredMessage =
  'La URL de la API no está configurada.'
const InvalidResponseMessage =
  'La API devolvió una respuesta no válida.'
const RequestFailedMessage =
  'No fue posible completar la solicitud.'
const DonationsPath = '/api/donations'

export class ApiError extends Error {
  readonly status: number

  constructor(
    message: string,
    status: number,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

function getApiUrl(): string {
  const apiUrl = import.meta.env.VITE_API_URL?.trim()

  if (!apiUrl) {
    throw new ApiError(ApiUrlRequiredMessage, 0)
  }

  return apiUrl.replace(/\/+$/, '')
}

function isEnvelope<T>(value: unknown): value is ApiResponseEnvelope<T> {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const envelope = value as Record<string, unknown>
  return (
    typeof envelope.succeeded === 'boolean' &&
    'data' in envelope &&
    (typeof envelope.error === 'string' || envelope.error === null)
  )
}

async function request<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const url = `${getApiUrl()}${path}`
  let response: Response

  try {
    response = await fetch(url, options)
  } catch {
    throw new ApiError(RequestFailedMessage, 0)
  }

  let body: unknown

  try {
    body = await response.json()
  } catch {
    throw new ApiError(InvalidResponseMessage, response.status)
  }

  if (!isEnvelope<T>(body)) {
    throw new ApiError(InvalidResponseMessage, response.status)
  }

  if (!response.ok || !body.succeeded) {
    throw new ApiError(
      body.error ?? RequestFailedMessage,
      response.status,
    )
  }

  if (body.data === null) {
    throw new ApiError(InvalidResponseMessage, response.status)
  }

  return body.data
}

function jsonOptions(method: 'POST' | 'PATCH', body: unknown): RequestInit {
  return {
    method,
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  }
}

export function listDonations(
  filters: DonationFilters = {},
): Promise<DonationLot[]> {
  const query = new URLSearchParams()

  if (filters.status) {
    query.set('status', filters.status)
  }

  if (filters.foodCategory) {
    query.set('foodCategory', filters.foodCategory)
  }

  const queryString = query.toString()
  const path = queryString
    ? `${DonationsPath}?${queryString}`
    : DonationsPath

  return request<DonationLot[]>(path)
}

export function getDonation(id: string): Promise<DonationLot> {
  return request<DonationLot>(
    `${DonationsPath}/${encodeURIComponent(id)}`,
  )
}

export function createDonation(
  donation: CreateDonationRequest,
): Promise<DonationLot> {
  return request<DonationLot>(
    DonationsPath,
    jsonOptions('POST', donation),
  )
}

export function claimDonation(
  id: string,
  claim: ClaimDonationRequest,
): Promise<DonationLot> {
  return request<DonationLot>(
    `${DonationsPath}/${encodeURIComponent(id)}/claim`,
    jsonOptions('POST', claim),
  )
}

export function changeDonationStatus(
  id: string,
  change: ChangeDonationStatusRequest,
): Promise<DonationLot> {
  return request<DonationLot>(
    `${DonationsPath}/${encodeURIComponent(id)}/status`,
    jsonOptions('PATCH', change),
  )
}
