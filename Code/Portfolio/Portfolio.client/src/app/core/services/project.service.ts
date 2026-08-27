import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Project, ProjectImage, UpsertProjectRequest } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly apiUrl = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/me`);
  }

  getMineById(id: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/me/${id}`);
  }

  getPublicByUserId(userId: number): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/public/${userId}`);
  }

  getPublicById(userId: number, projectId: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/public/${userId}/${projectId}`);
  }

  create(request: UpsertProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, request);
  }

  update(id: number, request: UpsertProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reorder(orderedIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reorder`, { orderedIds });
  }

  uploadImages(projectId: number, files: File[]): Observable<ProjectImage[]> {
    const formData = new FormData();
    files.forEach((file) => formData.append('Files', file));
    return this.http.post<ProjectImage[]>(`${this.apiUrl}/${projectId}/images`, formData);
  }

  deleteImage(projectId: number, imageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${projectId}/images/${imageId}`);
  }

  reorderImages(projectId: number, orderedIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${projectId}/images/reorder`, { orderedIds });
  }
}