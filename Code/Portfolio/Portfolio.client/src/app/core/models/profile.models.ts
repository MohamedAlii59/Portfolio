export interface ProfileResponse {
  id: number;
  fullName: string;
  email: string;
  slug: string;
  title: string | null;
  bio: string | null;
  profileImageUrl: string | null;
  phoneNumber: string | null;
  githubUrl: string | null;
  linkedInUrl: string | null;
  resumeFileName: string | null;
  hasResume: boolean;
}

export interface UpdateProfileRequest {
  fullName: string;
  title: string | null;
  bio: string | null;
  phoneNumber: string | null;
  githubUrl: string | null;
  linkedInUrl: string | null;
  slug: string;
}