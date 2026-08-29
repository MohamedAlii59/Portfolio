import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile.models';

@Component({
  selector: 'app-profile-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-editor.html',
  styleUrl: './profile-editor.css',
})
export class ProfileEditor implements OnInit {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);

  profile = signal<ProfileResponse | null>(null);
  isLoading = signal(true);
  isSaving = signal(false);
  saveMessage = signal<string | null>(null);
  saveError = signal<string | null>(null);

  isUploadingPhoto = signal(false);
  isUploadingResume = signal(false);

  form = this.fb.group({
    fullName: ['', [Validators.required]],
    title: [''],
    bio: [''],
    phoneNumber: [''],
    githubUrl: [''],
    linkedInUrl: [''],
    slug: ['yousef-ashour', [Validators.required, Validators.pattern('^[a-z0-9-]+$')]],
  });

  ngOnInit(): void {
    this.profileService.getMyProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.form.patchValue({
          fullName: profile.fullName,
          title: profile.title ?? '',
          bio: profile.bio ?? '',
          phoneNumber: profile.phoneNumber ?? '',
          githubUrl: profile.githubUrl ?? '',
          linkedInUrl: profile.linkedInUrl ?? '',
          slug: profile.slug || 'yousef-ashour',
        });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.saveMessage.set(null);
    this.saveError.set(null);

    const raw = this.form.getRawValue();
    const payload = {
      fullName: raw.fullName!,
      title: raw.title || null,
      bio: raw.bio || null,
      phoneNumber: raw.phoneNumber || null,
      githubUrl: raw.githubUrl || null,
      linkedInUrl: raw.linkedInUrl || null,
      slug: raw.slug!,
    };

    this.profileService.updateProfile(payload).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.isSaving.set(false);
        this.saveMessage.set('Saved successfully.');
        setTimeout(() => this.saveMessage.set(null), 3000);
      },
      error: (err) => {
        this.isSaving.set(false);
        this.saveError.set(err.error?.message ?? 'Something went wrong. Please check your inputs.');
      },
    });
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    this.isUploadingPhoto.set(true);
    this.profileService.uploadPhoto(input.files[0]).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.isUploadingPhoto.set(false);
      },
      error: () => this.isUploadingPhoto.set(false),
    });
  }

  onResumeSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    this.isUploadingResume.set(true);
    this.profileService.uploadResume(input.files[0]).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.isUploadingResume.set(false);
      },
      error: () => this.isUploadingResume.set(false),
    });
  }

  onDeleteResume(): void {
    if (!confirm('Remove your resume? You can upload a new one anytime.')) return;

    this.profileService.deleteResume().subscribe({
      next: () => {
        const current = this.profile();
        if (current) {
          this.profile.set({ ...current, hasResume: false, resumeFileName: null });
        }
      },
    });
  }
}