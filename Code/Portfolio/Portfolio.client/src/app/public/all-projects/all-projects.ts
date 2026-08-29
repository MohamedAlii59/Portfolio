import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { ProjectService } from '../../core/services/project.service';
import { ProfileResponse } from '../../core/models/profile.models';
import { Project } from '../../core/models/project.models';
import { RevealDirective } from '../../core/directives/reveal.directive';

@Component({
  selector: 'app-all-projects',
  standalone: true,
  imports: [CommonModule, RouterLink, RevealDirective],
  templateUrl: './all-projects.html',
  styleUrl: './all-projects.css',
})
export class AllProjects implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private titleService = inject(Title);
  private profileService = inject(ProfileService);
  private projectService = inject(ProjectService);

  profile = signal<ProfileResponse | null>(null);
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
        this.titleService.setTitle(profile.fullName);

        this.projectService.getPublicByUserId(profile.id).subscribe({
          next: (projects) => {
            const sorted = [...projects].sort((a, b) => {
              const dateA = a.projectDate ? new Date(a.projectDate).getTime() : 0;
              const dateB = b.projectDate ? new Date(b.projectDate).getTime() : 0;
              return dateB - dateA;
            });
            this.projects.set(sorted);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false),
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
}