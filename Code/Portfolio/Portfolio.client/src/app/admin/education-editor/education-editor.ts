import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EducationService } from '../../core/services/education.service';
import { EducationEntry } from '../../core/models/education.models';

@Component({
  selector: 'app-education-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './education-editor.html',
  styleUrl: './education-editor.css',
})
export class EducationEditor implements OnInit {
  private fb = inject(FormBuilder);
  private educationService = inject(EducationService);

  entries = signal<EducationEntry[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  editingId = signal<number | null>(null); // null = "add new" mode
  showForm = signal(false);

  form = this.fb.group({
    institution: ['', [Validators.required]],
    degree: [''],
    fieldOfStudy: [''],
    startDate: ['', [Validators.required]],
    endDate: [''],
    isPresent: [false],
    description: [''],
  });

  ngOnInit(): void {
    this.loadEntries();
  }

  loadEntries(): void {
    this.isLoading.set(true);
    this.educationService.getMine().subscribe({
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

  openEditForm(entry: EducationEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      institution: entry.institution,
      degree: entry.degree ?? '',
      fieldOfStudy: entry.fieldOfStudy ?? '',
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
      institution: raw.institution!,
      degree: raw.degree || null,
      fieldOfStudy: raw.fieldOfStudy || null,
      startDate: raw.startDate!,
      endDate: raw.isPresent ? null : (raw.endDate || null),
      description: raw.description || null,
    };

    this.isSaving.set(true);
    const id = this.editingId();
    const request$ = id ? this.educationService.update(id, payload) : this.educationService.create(payload);

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
    if (!confirm('Delete this education entry?')) return;

    this.educationService.delete(id).subscribe({
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

    this.educationService.reorder(current.map((e) => e.id)).subscribe();
  }
}