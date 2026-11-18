export interface Ticket {
  reservationId: number;
  ticketCode: string;
  seanceId: number;
  filmTitle: string;
  roomName: string;
  startsAtUtc: string;
  reservedAtUtc: string;
  seatNumbers: string[];
}
