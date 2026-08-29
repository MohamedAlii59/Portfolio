import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { EducationService } from '../../core/services/education.service';
import { ExperienceService } from '../../core/services/experience.service';
import { TechnologyService } from '../../core/services/technology.service';
import { ProjectService } from '../../core/services/project.service';
import { ProfileResponse } from '../../core/models/profile.models';
import { EducationEntry } from '../../core/models/education.models';
import { WorkExperienceEntry } from '../../core/models/experience.models';
import { Technology } from '../../core/models/technology.models';
import { Project } from '../../core/models/project.models';
import { environment } from '../../../environments/environment';
import { RevealDirective } from '../../core/directives/reveal.directive';

const FEATURED_PROJECT_COUNT = 3;

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, RevealDirective],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private titleService = inject(Title);
  private profileService = inject(ProfileService);
  private educationService = inject(EducationService);
  private experienceService = inject(ExperienceService);
  private technologyService = inject(TechnologyService);
  private projectService = inject(ProjectService);

  profile = signal<ProfileResponse | null>(null);
  education = signal<EducationEntry[]>([]);
  experience = signal<WorkExperienceEntry[]>([]);
  technologies = signal<Technology[]>([]);
  featuredProjects = signal<Project[]>([]);
  isLoading = signal(true);
  notFound = signal(false);
  currentYear = new Date().getFullYear();

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.notFound.set(true);
      this.isLoading.set(false);
      return;
    }

    this.profileService.getPublicProfile(slug).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.titleService.setTitle(profile.fullName);

        this.educationService.getForUser(profile.id).subscribe((e) => {
          this.education.set(this.sortNewestFirst(e));
        });

        this.experienceService.getForUser(profile.id).subscribe((e) => {
          this.experience.set(this.sortNewestFirst(e));
        });

        this.technologyService.getProfileTechnologies(profile.id).subscribe((t) => this.technologies.set(t));

        this.projectService.getPublicByUserId(profile.id).subscribe((p) => {
          const sorted = [...p].sort((a, b) => {
            const dateA = a.projectDate ? new Date(a.projectDate).getTime() : 0;
            const dateB = b.projectDate ? new Date(b.projectDate).getTime() : 0;
            return dateB - dateA;
          });
          this.featuredProjects.set(sorted.slice(0, FEATURED_PROJECT_COUNT));
          this.isLoading.set(false);
        });
      },
      error: () => {
        this.notFound.set(true);
        this.isLoading.set(false);
      },
    });
  }

  private sortNewestFirst<T extends { startDate: string }>(items: T[]): T[] {
    return [...items].sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime());
  }

  goToProject(projectId: number): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    this.router.navigate(['/u', slug, 'projects', projectId]);
  }

  get resumeDownloadUrl(): string | null {
    const p = this.profile();
    if (!p || !p.hasResume) return null;
    return `${environment.apiUrl}/profile/${p.slug}/resume/download`;
  }
}