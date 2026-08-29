import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl, Title } from '@angular/platform-browser';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { ProjectService } from '../../core/services/project.service';
import { Project } from '../../core/models/project.models';
import { RevealDirective } from '../../core/directives/reveal.directive';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, RevealDirective],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.css',
})
export class ProjectDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private titleService = inject(Title);
  private profileService = inject(ProfileService);
  private projectService = inject(ProjectService);
  private sanitizer = inject(DomSanitizer);

  project = signal<Project | null>(null);
  isLoading = signal(true);
  notFound = signal(false);
  activeImageIndex = signal(0);
  slug = signal<string>('');

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    const projectId = Number(this.route.snapshot.paramMap.get('projectId'));

    if (!slug || !projectId) {
      this.notFound.set(true);
      this.isLoading.set(false);
      return;
    }

    this.slug.set(slug);

    this.profileService.getPublicProfile(slug).subscribe({
      next: (profile) => {
        this.titleService.setTitle(profile.fullName);
        this.projectService.getPublicById(profile.id, projectId).subscribe({
          next: (project) => {
            this.project.set(project);
            this.isLoading.set(false);
          },
          error: () => {
            this.notFound.set(true);
            this.isLoading.set(false);
          },
        });
      },
      error: () => {
        this.notFound.set(true);
        this.isLoading.set(false);
      },
    });
  }

  setActiveImage(index: number): void {
    this.activeImageIndex.set(index);
  }

  get embedUrl(): SafeResourceUrl | null {
    const url = this.project()?.demoVideoUrl;
    if (!url) return null;

    const ytMatch = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([\w-]+)/);
    if (ytMatch) {
      return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube.com/embed/${ytMatch[1]}`);
    }

    const vimeoMatch = url.match(/vimeo\.com\/(\d+)/);
    if (vimeoMatch) {
      return this.sanitizer.bypassSecurityTrustResourceUrl(`https://player.vimeo.com/video/${vimeoMatch[1]}`);
    }

    return null;
  }
}