export type ReservationStatus = 'Pending' | 'Ready' | 'Fulfilled' | 'Cancelled';

export interface Reservation {
  id: string;
  memberId: string;
  memberName: string;
  bookId: string;
  bookTitle: string;
  branchId: string;
  branchName: string;
  reservedAtUtc: string;
  readyAtUtc: string | null;
  fulfilledAtUtc: string | null;
  cancelledAtUtc: string | null;
  status: ReservationStatus;
  queuePosition: number;
}

export interface CreateReservationRequest {
  memberId: string;
  bookId: string;
  branchId: string;
}

export interface CreateMyReservationRequest {
  bookId: string;
  branchId: string;
}
