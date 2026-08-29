import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ExperienceService } from '../../core/services/experience.service';
import { WorkExperienceEntry } from '../../core/models/experience.models';

@Component({
  selector: 'app-experience-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './experience-editor.html',
  styleUrl: './experience-editor.css',
})
export class ExperienceEditor implements OnInit {
  private fb = inject(FormBuilder);
  private experienceService = inject(ExperienceService);

  entries = signal<WorkExperienceEntry[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  editingId = signal<number | null>(null);
  showForm = signal(false);

  form = this.fb.group({
    company: ['', [Validators.required]],
    role: ['', [Validators.required]],
    startDate: ['', [Validators.required]],
    endDate: [''],
    isPresent: [false],
    description: [''],
  });

  get isPresentChecked(): boolean {
    return !!this.form.get('isPresent')?.value;
  }

  ngOnInit(): void {
    this.loadEntries();
  }

  loadEntries(): void {
    this.isLoading.set(true);
    this.experienceService.getMine().subscribe({
      next: (entries) => {
        this.entries.set(entries);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  openAddForm(): void {
    this.editingId.set(null);
    this.form.reset({ isPresent: false });
    this.showForm.set(true);
  }

  openEditForm(entry: WorkExperienceEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      company: entry.company,
      role: entry.role,
      startDate: entry.startDate.substring(0, 10),
      endDate: entry.endDate ? entry.endDate.substring(0, 10) : '',
      isPresent: entry.endDate === null,
      description: entry.description ?? '',
    });
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload = {
      company: raw.company!,
      role: raw.role!,
      startDate: raw.startDate!,
      endDate: raw.isPresent ? null : (raw.endDate || null),
      description: raw.description || null,
    };

    this.isSaving.set(true);
    const id = this.editingId();
    const request$ = id ? this.experienceService.update(id, payload) : this.experienceService.create(payload);

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showForm.set(false);
        this.loadEntries();
      },
      error: () => this.isSaving.set(false),
    });
  }

  onDelete(id: number): void {
    if (!confirm('Delete this experience entry?')) return;

    this.experienceService.delete(id).subscribe({
      next: () => this.loadEntries(),
    });
  }

  moveUp(index: number): void {
    if (index === 0) return;
    this.swapAndReorder(index, index - 1);
  }

  moveDown(index: number): void {
    if (index === this.entries().length - 1) return;
    this.swapAndReorder(index, index + 1);
  }

  private swapAndReorder(indexA: number, indexB: number): void {
    const current = [...this.entries()];
    [current[indexA], current[indexB]] = [current[indexB], current[indexA]];
    this.entries.set(current);

    this.experienceService.reorder(current.map((e) => e.id)).subscribe();
  }
}