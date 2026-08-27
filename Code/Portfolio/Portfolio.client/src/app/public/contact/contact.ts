import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
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

  goBack(): void {
    this.router.navigate(['/u', this.slug()]);
  }
}