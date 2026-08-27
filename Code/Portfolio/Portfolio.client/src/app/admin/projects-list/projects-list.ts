import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProjectService } from '../../core/services/project.service';
import { Project } from '../../core/models/project.models';

@Component({
  selector: 'app-projects-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './projects-list.html',
  styleUrl: './projects-list.css',
})
export class ProjectsList implements OnInit {
  private projectService = inject(ProjectService);
  private router = inject(Router);

  projects = signal<Project[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoading.set(true);
    this.projectService.getMine().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  goToNew(): void {
    this.router.navigate(['/admin/projects/new']);
  }

  goToEdit(id: number): void {
    this.router.navigate(['/admin/projects', id]);
  }

  onDelete(id: number, event: Event): void {
    event.stopPropagation();
    if (!confirm('Delete this project? This also removes all its images permanently.')) return;

    this.projectService.delete(id).subscribe({
      next: () => this.loadProjects(),
    });
  }

  moveUp(index: number, event: Event): void {
    event.stopPropagation();
    if (index === 0) return;
    this.swapAndReorder(index, index - 1);
  }

  moveDown(index: number, event: Event): void {
    event.stopPropagation();
    if (index === this.projects().length - 1) return;
    this.swapAndReorder(index, index + 1);
  }

  private swapAndReorder(indexA: number, indexB: number): void {
    const current = [...this.projects()];
    [current[indexA], current[indexB]] = [current[indexB], current[indexA]];
    this.projects.set(current);

    this.projectService.reorder(current.map((p) => p.id)).subscribe();
  }
}