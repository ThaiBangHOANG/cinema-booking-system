export interface SeanceOccupancy {
  seanceId: number;
  filmTitle: string;
  roomName: string;
  startsAtUtc: string;
  totalSeats: number;
  reservedSeats: number;
  availableSeats: number;
  occupancyRate: number;
}

export interface OccupancySummary {
  totalSeances: number;
  totalSeats: number;
  reservedSeats: number;
  availableSeats: number;
  occupancyRate: number;
  seances: SeanceOccupancy[];
}

export interface DailyAffluence {
  date: string; // DateOnly serialized as yyyy-MM-dd by ASP.NET Core
  isWeekend: boolean;
  isFestival: boolean;
  totalSeances: number;
  totalSeats: number;
  reservedSeats: number;
  occupancyRate: number;
  alertLevel: string;
}

export interface HourlyAffluence {
  hourUtc: number;
  totalSeances: number;
  reservedSeats: number;
  occupancyRate: number;
}

export interface AffluenceReport {
  startDate: string;
  endDate: string;
  totalReservedSeats: number;
  globalOccupancyRate: number;
  dailyTrends: DailyAffluence[];
  hourlyTrends: HourlyAffluence[];
}

export interface AffluenceQuery {
  days: number;
  festivalStart?: string;
  festivalEnd?: string;
}
