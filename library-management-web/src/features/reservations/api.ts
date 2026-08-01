import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type { CreateMyReservationRequest, CreateReservationRequest, Reservation, ReservationStatus } from './types';

export interface GetReservationsParams {
  status?: ReservationStatus;
  pageNumber: number;
  pageSize: number;
}

export async function getReservations(params: GetReservationsParams): Promise<PagedResult<Reservation>> {
  const { data } = await api.get<PagedResult<Reservation>>('/api/reservations', {
    params: {
      Status: params.status || undefined,
      PageNumber: params.pageNumber,
      PageSize: params.pageSize,
    },
  });
  return data;
}

export interface GetMyReservationsParams {
  pageNumber: number;
  pageSize: number;
}

export async function getMyReservations(params: GetMyReservationsParams): Promise<PagedResult<Reservation>> {
  const { data } = await api.get<PagedResult<Reservation>>('/api/reservations/me', {
    params: { PageNumber: params.pageNumber, PageSize: params.pageSize },
  });
  return data;
}

export async function createReservation(request: CreateReservationRequest): Promise<Reservation> {
  const { data } = await api.post<Reservation>('/api/reservations', request);
  return data;
}

export async function createMyReservation(request: CreateMyReservationRequest): Promise<Reservation> {
  const { data } = await api.post<Reservation>('/api/reservations/me', request);
  return data;
}

export async function fulfillReservation(id: string): Promise<void> {
  await api.post(`/api/reservations/${id}/fulfill`);
}

export async function cancelReservation(id: string): Promise<void> {
  await api.post(`/api/reservations/${id}/cancel`);
}
