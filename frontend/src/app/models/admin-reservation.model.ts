export interface AdminReservation {
  id: number;
  ticketCode: string;
  userId: number;
  userEmail: string;
  seanceId: number;
  filmTitle: string;
  roomName: string;
  startsAtUtc: string;
  reservedAtUtc: string;
  seatNumbers: string[];
}

export interface AdminReservationList {
  total: number;
  items: AdminReservation[];
}
