export interface SeatAvailability {
  seatId: number;
  seatNumber: string;
  isAvailable: boolean;
}

export interface SeanceSeats {
  seanceId: number;
  roomId: number;
  roomName: string;
  totalSeats: number;
  availableSeats: number;
  seats: SeatAvailability[];
}
