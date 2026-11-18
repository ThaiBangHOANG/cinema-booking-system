export interface CreateReservationRequest {
  seanceId: number;
  seatIds: number[];
}

export interface ReservationResult {
  reservationId: number;
  seanceId: number;
  ticketCode: string;
  reservedAtUtc: string;
  seatIds: number[];
}
