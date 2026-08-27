import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TechnologyService } from '../../core/services/technology.service';
import { ProfileService } from '../../core/services/profile.service';
import { Technology } from '../../core/models/technology.models';

@Component({
  selector: 'app-technologies-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './technologies-manager.html',
  styleUrl: './technologies-manager.css',
})
export class TechnologiesManager implements OnInit {
  private fb = inject(FormBuilder);
  private technologyService = inject(TechnologyService);
  private profileService = inject(ProfileService);

  allTechnologies = signal<Technology[]>([]);
  myTechnologyIds = signal<Set<number>>(new Set());
  isLoading = signal(true);
  isCreating = signal(false);
  showAddForm = signal(false);
  selectedIconName = signal<string | null>(null);
  createError = signal<string | null>(null);

  private selectedIconFile: File | null = null;

  form = this.fb.group({
    name: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.isLoading.set(true);

    this.profileService.getMyProfile().subscribe({
      next: (profile) => {
        this.technologyService.getAll().subscribe({
          next: (allTechs) => {
            this.allTechnologies.set(allTechs);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false),
        });

        this.technologyService.getProfileTechnologies(profile.id).subscribe({
          next: (myTechs) => {
            this.myTechnologyIds.set(new Set(myTechs.map((t) => t.id)));
          },
        });
      },
      error: () => this.isLoading.set(false),
    });
  }

  isOnProfile(techId: number): boolean {
    return this.myTechnologyIds().has(techId);
  }

  toggleTechnology(tech: Technology): void {
    const isCurrentlyOn = this.isOnProfile(tech.id);
    const request$ = isCurrentlyOn
      ? this.technologyService.removeFromProfile(tech.id)
      : this.technologyService.addToProfile(tech.id);

    request$.subscribe({
      next: () => {
        const current = new Set(this.myTechnologyIds());
        isCurrentlyOn ? current.delete(tech.id) : current.add(tech.id);
        this.myTechnologyIds.set(current);
      },
    });
  }

  onIconSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedIconFile = input.files[0];
      this.selectedIconName.set(input.files[0].name);
    }
  }

  onCreate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isCreating.set(true);
    this.createError.set(null);

    const name = this.form.value.name!;
    this.technologyService.create(name, this.selectedIconFile).subscribe({
      next: () => {
        this.isCreating.set(false);
        this.showAddForm.set(false);
        this.form.reset();
        this.selectedIconFile = null;
        this.selectedIconName.set(null);
        this.loadAll();
      },
      error: (err) => {
        this.isCreating.set(false);
        this.createError.set(err.error?.message ?? 'Could not create technology.');
      },
    });
  }

  onDelete(tech: Technology): void {
    if (!confirm(`Delete "${tech.name}"? This only works if it's not used anywhere.`)) return;

    this.technologyService.delete(tech.id).subscribe({
      next: () => this.loadAll(),
      error: (err) => {
        alert(err.error?.message ?? 'Could not delete this technology.');
      },
    });
  }
}