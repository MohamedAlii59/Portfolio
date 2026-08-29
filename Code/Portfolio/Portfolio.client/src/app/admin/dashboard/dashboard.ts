import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterOutlet, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, RouterOutlet, RouterLinkActive],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private authService = inject(AuthService);
  private profileService = inject(ProfileService);
  private router = inject(Router);

  fullName = this.authService.getFullName();
  slug = this.authService.getSlug();
  profilePhotoUrl = signal<string | null>(null);

  ngOnInit(): void {
    // Fetch the real profile photo for the sidebar avatar (fullName/slug come
    // from localStorage already, but the photo needs a fresh API call).
    this.profileService.getMyProfile().subscribe({
      next: (profile) => this.profilePhotoUrl.set(profile.profileImageUrl),
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/admin/login']);
  }
}