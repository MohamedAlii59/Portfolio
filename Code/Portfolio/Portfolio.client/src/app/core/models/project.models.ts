export interface ProjectImage {
  id: number;
  imageUrl: string;
  displayOrder: number;
}

export interface Technology {
  id: number;
  name: string;
  iconUrl: string | null;
}

export interface Project {
  id: number;
  title: string;
  description: string | null;
  shortDescription: string | null;
  projectDate: string | null;
  demoVideoUrl: string | null;
  githubUrl: string | null;
  projectUrl: string | null;
  displayOrder: number;
  images: ProjectImage[];
  technologies: Technology[];
}

export interface UpsertProjectRequest {
  title: string;
  description: string | null;
  shortDescription: string | null;
  projectDate: string | null;
  demoVideoUrl: string | null;
  githubUrl: string | null;
  projectUrl: string | null;
  technologyIds: number[];
}