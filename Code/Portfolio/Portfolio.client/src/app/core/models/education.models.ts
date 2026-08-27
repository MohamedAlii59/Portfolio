export interface EducationEntry {
  id: number;
  institution: string;
  degree: string | null;
  fieldOfStudy: string | null;
  startDate: string; // ISO date string
  endDate: string | null;
  description: string | null;
  displayOrder: number;
}

export interface UpsertEducationRequest {
  institution: string;
  degree: string | null;
  fieldOfStudy: string | null;
  startDate: string;
  endDate: string | null;
  description: string | null;
}