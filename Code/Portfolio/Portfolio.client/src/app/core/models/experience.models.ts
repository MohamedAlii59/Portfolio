export interface WorkExperienceEntry {
  id: number;
  company: string;
  role: string;
  startDate: string;
  endDate: string | null;
  description: string | null;
  displayOrder: number;
}

export interface UpsertWorkExperienceRequest {
  company: string;
  role: string;
  startDate: string;
  endDate: string | null;
  description: string | null;
}