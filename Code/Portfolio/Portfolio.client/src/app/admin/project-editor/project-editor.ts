import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../../core/services/project.service';
import { TechnologyService } from '../../core/services/technology.service';
import { Project, ProjectImage } from '../../core/models/project.models';
import { Technology } from '../../core/models/technology.models';

@Component({
  selector: 'app-project-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './project-editor.html',
  styleUrl: './project-editor.css',
})
export class ProjectEditor implements OnInit {
  private fb = inject(FormBuilder);
  private projectService = inject(ProjectService);
  private technologyService = inject(TechnologyService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  projectId = signal<number | null>(null); // null = creating new
  isLoading = signal(true);
  isSaving = signal(false);
  saveError = signal<string | null>(null);

  images = signal<ProjectImage[]>([]);
  isUploadingImages = signal(false);

  allTechnologies = signal<Technology[]>([]);
  selectedTechIds = signal<Set<number>>(new Set());

  form = this.fb.group({
    title: ['', [Validators.required]],
    shortDescription: [''],
    description: [''],
    projectDate: [''],
    demoVideoUrl: [''],
    githubUrl: [''],
    projectUrl: [''],
  });

  ngOnInit(): void {
    this.technologyService.getAll().subscribe({
      next: (techs) => this.allTechnologies.set(techs),
    });

    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam && idParam !== 'new') {
      const id = Number(idParam);
      this.projectId.set(id);
      this.loadProject(id);
    } else {
      this.isLoading.set(false);
    }
  }

  loadProject(id: number): void {
    this.isLoading.set(true);
    this.projectService.getMineById(id).subscribe({
      next: (project) => {
        this.form.patchValue({
          title: project.title,
          shortDescription: project.shortDescription ?? '',
          description: project.description ?? '',
          projectDate: project.projectDate ? project.projectDate.substring(0, 10) : '',
          demoVideoUrl: project.demoVideoUrl ?? '',
          githubUrl: project.githubUrl ?? '',
          projectUrl: project.projectUrl ?? '',
        });
        this.images.set(project.images);
        this.selectedTechIds.set(new Set(project.technologies.map((t) => t.id)));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  isTechSelected(id: number): boolean {
    return this.selectedTechIds().has(id);
  }

  toggleTech(id: number): void {
    const current = new Set(this.selectedTechIds());
    current.has(id) ? current.delete(id) : current.add(id);
    this.selectedTechIds.set(current);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload = {
      title: raw.title!,
      shortDescription: raw.shortDescription || null,
      description: raw.description || null,
      projectDate: raw.projectDate || null,
      demoVideoUrl: raw.demoVideoUrl || null,
      githubUrl: raw.githubUrl || null,
      projectUrl: raw.projectUrl || null,
      technologyIds: Array.from(this.selectedTechIds()),
    };

    this.isSaving.set(true);
    this.saveError.set(null);

    const id = this.projectId();
    const request$ = id ? this.projectService.update(id, payload) : this.projectService.create(payload);

    request$.subscribe({
      next: (project) => {
        this.isSaving.set(false);
        if (!id) {
          // Just created — switch into "edit" mode on this same page so
          // image upload (which needs a real project Id) becomes available.
          this.projectId.set(project.id);
          this.router.navigate(['/admin/projects', project.id], { replaceUrl: true });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.saveError.set(err.error?.message ?? 'Something went wrong.');
      },
    });
  }

  onImagesSelected(event: Event): void {
    const id = this.projectId();
    if (!id) return; // shouldn't happen — upload UI only shows once a project exists

    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    this.isUploadingImages.set(true);
    this.projectService.uploadImages(id, Array.from(input.files)).subscribe({
      next: (newImages) => {
        this.images.set([...this.images(), ...newImages]);
        this.isUploadingImages.set(false);
      },
      error: () => this.isUploadingImages.set(false),
    });
  }

  onDeleteImage(imageId: number): void {
    const id = this.projectId();
    if (!id) return;
    if (!confirm('Delete this image?')) return;

    this.projectService.deleteImage(id, imageId).subscribe({
      next: () => this.images.set(this.images().filter((img) => img.id !== imageId)),
    });
  }

  moveImageLeft(index: number): void {
    if (index === 0) return;
    this.swapImages(index, index - 1);
  }

  moveImageRight(index: number): void {
    if (index === this.images().length - 1) return;
    this.swapImages(index, index + 1);
  }

  private swapImages(indexA: number, indexB: number): void {
    const id = this.projectId();
    if (!id) return;

    const current = [...this.images()];
    [current[indexA], current[indexB]] = [current[indexB], current[indexA]];
    this.images.set(current);

    this.projectService.reorderImages(id, current.map((img) => img.id)).subscribe();
  }

  goBack(): void {
    this.router.navigate(['/admin/projects']);
  }
}