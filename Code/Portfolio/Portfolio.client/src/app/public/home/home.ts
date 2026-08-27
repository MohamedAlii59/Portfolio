import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
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

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private profileService = inject(ProfileService);
  private educationService = inject(EducationService);
  private experienceService = inject(ExperienceService);
  private technologyService = inject(TechnologyService);
  private projectService = inject(ProjectService);

  profile = signal<ProfileResponse | null>(null);
  education = signal<EducationEntry[]>([]);
  experience = signal<WorkExperienceEntry[]>([]);
  technologies = signal<Technology[]>([]);
  projects = signal<Project[]>([]);
  isLoading = signal(true);
  notFound = signal(false);

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

        this.educationService.getForUser(profile.id).subscribe((e) => this.education.set(e));
        this.experienceService.getForUser(profile.id).subscribe((e) => this.experience.set(e));
        this.technologyService.getProfileTechnologies(profile.id).subscribe((t) => this.technologies.set(t));
        this.projectService.getPublicByUserId(profile.id).subscribe((p) => {
          this.projects.set(p);
          this.isLoading.set(false);
        });
      },
      error: () => {
        this.notFound.set(true);
        this.isLoading.set(false);
      },
    });
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