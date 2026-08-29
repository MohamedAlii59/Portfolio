import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile.models';
import { environment } from '../../../environments/environment';
import { RevealDirective } from '../../core/directives/reveal.directive';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, RouterLink, RevealDirective],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact implements OnInit {
  private route = inject(ActivatedRoute);
  private titleService = inject(Title);
  private profileService = inject(ProfileService);

  profile = signal<ProfileResponse | null>(null);
  isLoading = signal(true);
  notFound = signal(false);
  slug = signal<string>('');

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.notFound.set(true);
      this.isLoading.set(false);
      return;
    }

    this.slug.set(slug);

    this.profileService.getPublicProfile(slug).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.titleService.setTitle(profile.fullName);
        this.isLoading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.isLoading.set(false);
      },
    });
  }

  get resumeDownloadUrl(): string | null {
    const p = this.profile();
    if (!p || !p.hasResume) return null;
    return `${environment.apiUrl}/profile/${p.slug}/resume/download`;
  }
}