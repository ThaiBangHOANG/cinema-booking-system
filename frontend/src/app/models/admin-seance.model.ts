export interface AdminSeance {
  id: number;
  filmId: number;
  filmTitle: string;
  roomId: number;
  roomName: string;
  startsAtUtc: string;
}

export interface AdminSeanceRequest {
  filmId: number;
  roomId: number;
  startsAtUtc: string;
}

export interface RoomOption {
  id: number;
  name: string;
}
